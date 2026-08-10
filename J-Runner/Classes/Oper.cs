using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace JRunner
{
    public static class Oper
    {
        /// <summary>
        /// File Manipulation - Open File - Save File
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="size"></param>
        /// <param name="wantedsize"></param>
        /// <returns>byte[]</returns>
        #region file manipulation

        /// <summary>
        /// Fills <paramref name="buffer"/> with exactly <paramref name="count"/> bytes.
        /// Stream.Read may legally return fewer bytes than asked for, so it has to be looped
        /// rather than called once.
        ///
        /// Throws EndOfStreamException on a short read, deliberately: the per-byte
        /// BinaryReader.ReadByte() loop this replaced threw the same exception when a file
        /// was shorter than requested, and both callers sit inside a try/catch that turns
        /// that into a null return. Swallowing it here would hand callers a half-filled
        /// buffer they'd treat as a complete image - a behaviour change, and a dangerous one
        /// in a tool that writes NAND.
        /// </summary>
        private static void ReadExactly(Stream stream, byte[] buffer, int count)
        {
            int done = 0;
            while (done < count)
            {
                int n = stream.Read(buffer, done, count - done);
                if (n <= 0) throw new EndOfStreamException();
                done += n;
            }
        }

        public static byte[] openfile(string filename, ref long size, int wantedsize)
        {
            try
            {
                FileInfo info = new FileInfo(filename);
                if (wantedsize == 0 || wantedsize > info.Length)
                {
                    size = info.Length;
                }
                else
                    size = wantedsize;
                // Was a per-byte BinaryReader.ReadByte() loop - 69,206,016 virtual calls to
                // load a 64MB image, each with its own bounds check and refill test. Reads
                // in bulk instead; the stream fills the array directly.
                byte[] data = new byte[size];
                using (FileStream infile = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    ReadExactly(infile, data, (int)size);
                }
                return data;
            }
            catch (FileNotFoundException ex) { Console.WriteLine("File {0} not found!", filename); if (variables.debugme) Console.WriteLine(ex.ToString()); }
            catch (Exception ex) { if (variables.debugme) Console.WriteLine(ex.ToString()); }
            return null;
        }

        public static byte[] openfilefromoffset(string filename, ref long size, int wantedsize, int offset)
        {
            try
            {
                if (variables.debugme) Console.WriteLine("filename: {0} - wantedsize: {1:X} - offset: {2:X}", filename, wantedsize, offset);
                FileInfo info = new FileInfo(filename);
                if (wantedsize == 0 || wantedsize + offset > info.Length)
                {
                    if (info.Length - offset < 0) size = 0;
                    else size = info.Length - offset;
                }
                else
                    size = wantedsize;
                if (variables.debugme) Console.WriteLine("size: {0:X}", size);
                byte[] data = new byte[size];
                using (FileStream infile = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    infile.Seek(offset, SeekOrigin.Begin);
                    ReadExactly(infile, data, (int)size);
                }
                return data;
            }
            catch (FileNotFoundException ex) { Console.WriteLine("File {0} not found!", filename); if (variables.debugme) Console.WriteLine(ex.ToString()); }
            catch (Exception ex) { if (variables.debugme) Console.WriteLine(ex.ToString()); }
            return null;
        }

        public static bool savefile(byte[] image, string name)
        {
            try
            {
                // The mirror of the read-side fix: this was a per-byte
                // BinaryWriter.Write(byte) loop, so saving a 64MB image meant 69,206,016
                // calls, each with a virtual dispatch and a buffer check. One bulk Write
                // moves the whole array. 60 call sites use this, including every full-image
                // save.
                //
                // using-blocks rather than explicit Close(): the old code leaked both
                // handles if the write threw, and the catch below swallowed the exception -
                // so a failed save could leave the file locked until the process exited.
                using (FileStream kvdf = new FileStream(name, FileMode.Create, FileAccess.Write))
                {
                    if (image != null && image.Length > 0) kvdf.Write(image, 0, image.Length);
                }
                return true;
            }
            catch (Exception ex) { if (variables.debugme) Console.WriteLine(ex.ToString()); return false; }

        }

        public static string FilePickerInitialPath(string input)
        {
            if (input.Length > 0)
            {
                try
                {
                    FileAttributes pathattr = File.GetAttributes(Path.GetFullPath(input));
                    if (pathattr.HasFlag(FileAttributes.Directory))
                    {
                        return Path.GetFullPath(input);
                    }
                    else return Path.GetDirectoryName(input);
                }
                catch
                {
                    return "";
                }
            }
            else return "";
        }

        #endregion

        /// <summary>
        /// addtoflash - padto
        /// </summary>
        /// <param name="image"></param>
        /// <param name="padding"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        #region addtoflash - padto

        public static byte[] padto(byte[] image, byte padding, int length)
        {
            byte[] newimage = new byte[length];
            for (int i = 0; i < length; i++)
            {
                if (i < image.Length) newimage[i] = image[i];
                else newimage[i] = padding;
            }
            return newimage;

        }

        public static byte[] addtoflash_v2(byte[] image, byte[] secondimage)
        {
            byte[] tempimage = new byte[image.Length + secondimage.Length];
            Buffer.BlockCopy(image, 0, tempimage, 0, image.Length);
            Buffer.BlockCopy(secondimage, 0, tempimage, image.Length, secondimage.Length);
            return tempimage;
        }

        /// <summary>
        /// Reassembles a crypto header: bytes 0x00-0x0F from <paramref name="head"/>,
        /// 0x10-0x1F from <paramref name="middle"/>, and 0x20 onwards from <paramref
        /// name="body"/> (which starts at its own offset 0).
        ///
        /// Replaces seven copies of the same loop in Nand.cs, each of which tested every
        /// byte index against two thresholds to decide which of three fixed regions it came
        /// from - over a whole image. The regions are constant, so three block copies do the
        /// same work without touching bytes individually. The seven differed only in which
        /// array supplied the middle 16 bytes, which is the parameter.
        /// </summary>
        /// <param name="midStart">Where the middle region begins - 0x10 for most headers,
        /// 0x20 for the CB variant that carries a larger nonce.</param>
        /// <param name="bodyStart">Where the body begins - correspondingly 0x20 or 0x30.</param>
        public static byte[] assembleCrypto(byte[] head, byte[] middle, byte[] body, int totalLength,
                                            int midStart = 0x10, int bodyStart = 0x20)
        {
            byte[] result = new byte[totalLength];

            int headLen = Math.Min(midStart, totalLength);
            if (headLen > 0 && head != null)
                Buffer.BlockCopy(head, 0, result, 0, Math.Min(headLen, head.Length));

            int midLen = Math.Min(bodyStart - midStart, Math.Max(0, totalLength - midStart));
            if (midLen > 0 && middle != null)
                Buffer.BlockCopy(middle, 0, result, midStart, Math.Min(midLen, middle.Length));

            int bodyLen = Math.Max(0, totalLength - bodyStart);
            if (bodyLen > 0 && body != null)
                Buffer.BlockCopy(body, 0, result, bodyStart, Math.Min(bodyLen, body.Length));

            return result;
        }

        public static byte[] addtoflash_v1(byte[] image, byte[] secondimage)
        {
            // Two element-by-element copy loops replaced with block copies. Buffer.BlockCopy
            // moves whole runs at a time instead of one bounds-checked array index per byte,
            // which matters here because this concatenates full NAND images.
            byte[] tempimage = new byte[image.Length + secondimage.Length];
            if (image.Length > 0) Buffer.BlockCopy(image, 0, tempimage, 0, image.Length);
            if (secondimage.Length > 0) Buffer.BlockCopy(secondimage, 0, tempimage, image.Length, secondimage.Length);
            return tempimage;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        #region usefull functions

        public static byte[] endianness(byte[] a1)
        {
            if (a1 != null) Array.Reverse(a1);
            return a1;
        }

        public static bool ByteArrayCompare(byte[] a1, byte[] a2, int size = 0)
        {
            if (a1 == null || a2 == null) return false;
            if (size == 0)
            {
                size = a1.Length;
                if (a1.Length != a2.Length)
                    return false;
            }

            for (int i = 0; i < size; i++)
                if (a1[i] != a2[i])
                    return false;

            return true;
        }
        public static bool ByteArrayCompare(byte[] a1, byte[] a2, int a1startoffset, int a2startoffset, int size)
        {
            if (a1 == null || a2 == null) return false;
            if ((a1.Length - a1startoffset < size || a2.Length - a2startoffset < size) && a1.Length - a1startoffset != a2.Length - a2startoffset) return false;

            int length = a1.Length - a1startoffset < size ? a1.Length - a1startoffset : size;

            for (int i = 0; i < length; i++)
                if (a1[i + a1startoffset] != a2[i + a2startoffset])
                    return false;

            return true;
        }


        public static int ByteArrayFindPattern(byte[] data, byte?[] pattern)
        {
            if (data == null || pattern == null)
            {
                return -1;
            }

            if (pattern.Length == 0 || data.Length < pattern.Length)
            {
                return -1;
            }

            for (int i = 0; i <= data.Length - pattern.Length; i++)
            {
                bool match = true;

                for (int j = 0; j < pattern.Length; j++)
                {
                    byte? p = pattern[j];

                    // If not a wildcard, compare
                    if (p.HasValue && data[i + j] != p.Value)
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Copies the 0x200-byte data half out of every 0x210-byte page in <paramref
        /// name="image"/>, dropping the 0x10-byte spare/ECC area - i.e. the inverse of
        /// addecc. Optionally starting at an offset and limited to a byte count.
        ///
        /// Replaces the
        ///     res = concatByteArrays(res, returnportion(image, counter, 0x200), res.Length, 0x200)
        /// idiom that appeared at six call sites. That form reallocated and re-copied the
        /// whole accumulated result on every page, so stripping a 64MB image (131,072 pages)
        /// copied ~4.4 TB and allocated ~131,000 arrays to produce 67MB. This sizes the
        /// output once and writes each page straight into place.
        /// </summary>
        /// <param name="requireFullPage">
        /// Two loop shapes existed in the original code and they are NOT equivalent:
        ///   for (c = 0; c &lt; len;        c += 0x210)   // emits a final partial page
        ///   for (c = 0; c + 496 &lt; len;  c += 0x210)   // skips it
        /// On an image whose length isn't a whole number of pages these produce different
        /// page counts, so the caller has to say which it wants. Passing true reproduces the
        /// "+ 496" form.
        /// </param>
        public static byte[] stripEcc(byte[] image, int start = 0, int length = -1, bool requireFullPage = false)
        {
            if (image == null) return new byte[0];

            int end = length < 0 ? image.Length : Math.Min(image.Length, start + length);
            // The "+ 496" guard stops 496 bytes short of the end, not 0x200.
            if (requireFullPage) end -= 496;
            if (start < 0) start = 0;
            if (end <= start) return new byte[0];

            // A page is emitted for every loop position, including a final one with fewer
            // than 0x200 bytes left. That matters: returnportion() zero-pads a short read up
            // to the requested length, so the original loops emitted a full 0x200-byte page
            // there, padded with zeros - and the callers' offsets depend on that page being
            // present. Dropping it would shift everything after it.
            int pages = 0;
            for (int c = start; c < end; c += 0x210) pages++;

            byte[] result = new byte[pages * 0x200];
            int w = 0;
            for (int c = start; c < end; c += 0x210)
            {
                int avail = image.Length - c;
                if (avail > 0x200) avail = 0x200;
                if (avail > 0) Buffer.BlockCopy(image, c, result, w, avail);
                // Anything past the end stays zero, matching returnportion's padding.
                w += 0x200;
            }
            return result;
        }

        public static byte[] concatByteArrays(byte[] byteArray1, byte[] byteArray2, int array1Length, int array2Length)
        {
            int totalLength = array1Length + array2Length;
            byte[] newByteArray = new byte[totalLength];
            int i = 0;

            while (i < array1Length)
                newByteArray[i] = byteArray1[i++];

            while (i < totalLength)
            {
                newByteArray[i] = byteArray2[i - array1Length];
                i++;
            }
            return newByteArray;
            //byteArray1 = newByteArray; // can assignment be made for arrays?  
            // if not, return newByteArray
        }
        public static byte[] concatByte(byte[] byteArray1, byte byteArray2, int array1Length)
        {
            int totalLength = array1Length + 1;
            byte[] newByteArray = new byte[totalLength];
            int i = 0;

            while (i < array1Length)
                newByteArray[i] = byteArray1[i++];

            while (i < totalLength)
            {
                newByteArray[i] = byteArray2;
                i++;
            }
            return newByteArray;
            //byteArray1 = newByteArray; // can assignment be made for arrays?  
            // if not, return newByteArray
        }

        public static byte[] returnportion(ref byte[] data, int offset, int count)
        {
            if (data == null) return null;
            if (count < 0) count = 0;
            byte[] templist = new byte[count];
            if (offset + count > data.Length)
            {
                if (variables.debugme) Console.WriteLine("Bigger - offset: {0:X} - count {1:X}", offset, count);
                count = data.Length - offset;
            }
            if (count <= data.Length && count >= 0)
            {
                Buffer.BlockCopy(data, offset, templist, 0x00, count);
            }
            return templist;
        }
        public static byte[] returnportion(byte[] data, int offset, int count)
        {
            if (data == null) return null;
            if (count < 0) count = 0;
            byte[] templist = new byte[count];
            if (offset + count > data.Length)
            {
                if (variables.debugme) Console.WriteLine("Bigger - offset: {0:X} - count {1:X}", offset, count);
                count = data.Length - offset;
            }
            if (count <= data.Length && count >= 0)
            {
                Buffer.BlockCopy(data, offset, templist, 0x00, count);
            }
            return templist;
        }
        public static byte[] returnportion_ecc(byte[] data, int offset, int count, int startindex = 0)
        {
            if (data == null) return null;
            byte[] templist = new byte[count];
            int i = 0;
            int remain = 0;
            if (startindex != 0)
            {
                remain = 0x200 - (startindex % 0x210);
                Buffer.BlockCopy(data, offset, templist, 0, remain);
                i = remain + 0x10;
            }

            if ((count * 0x210) / 0x200 <= data.Length && count >= 0)
            {
                for (; i < (count * 0x210) / 0x200; i += 0x210)
                {
                    Buffer.BlockCopy(data, offset + i, templist, (i * 0x200) / 0x210, 0x200);
                }
            }
            return templist;
        }

        public static bool allsame(byte[] s, byte n)
        {
            foreach (byte x in s)
            {
                if (x != n) return false;
            }
            return true;
        }

        public static string ByteArrayToString(byte[] ba, int startindex = 0, int length = 0)
        {
            if (ba == null) return "";

            // The first line used to be an unconditional BitConverter.ToString(ba) whose
            // result was then overwritten by the branch below - so every call converted the
            // whole array once for nothing. On a 64MB image that alone built a ~200MB string
            // and threw it away.
            //
            // BitConverter.ToString also produces "AA-BB-CC" and then Replace("-","")
            // allocated a second string to strip the separators. Writing the hex directly
            // does it in one pass with one allocation.
            int start = startindex;
            int count = length == 0 ? ba.Length - startindex : length;
            if (count <= 0) return "";

            char[] chars = new char[count * 2];
            for (int i = 0; i < count; i++)
            {
                byte b = ba[start + i];
                int hi = b >> 4, lo = b & 0xF;
                chars[i * 2] = (char)(hi < 10 ? '0' + hi : 'A' + (hi - 10));
                chars[i * 2 + 1] = (char)(lo < 10 ? '0' + lo : 'A' + (lo - 10));
            }
            return new string(chars);
        }
        public static string ByteArrayToString_v2(byte[] ba, int startindex = 0, int length = 0)
        {
            if (ba == null) return "";
            return Encoding.ASCII.GetString(ba, startindex, length == 0 ? ba.Length : length);
        }
        public static decimal ByteArrayToDecimal(byte[] src)
        {

            // Create a MemoryStream containing the byte array
            using (MemoryStream stream = new MemoryStream(src))
            {

                // Create a BinaryReader to read the decimal from the stream
                using (BinaryReader reader = new BinaryReader(stream))
                {

                    // Read and return the decimal from the 
                    // BinaryReader/MemoryStream
                    return reader.ReadDecimal();
                }
            }
        }
        public static int ByteArrayToInt(byte[] value)
        {
            // Was: Convert.ToInt32(ByteArrayToString(value), 16) - it built a hex string
            // from the bytes and then parsed that string back into a number, for every one
            // of 79 call sites. Shifting the bytes together does the same thing with no
            // string, no allocation and no parse.
            //
            // Semantics kept deliberately: big-endian (the hex string read left to right),
            // an empty/null array gives 0 (Convert.ToInt32("", 16) does too), and only the
            // low 4 bytes survive - Convert.ToInt32 would have thrown on a longer array, so
            // callers never pass one. All current callers pass 2 or 4 bytes.
            if (value == null || value.Length == 0) return 0;

            int result = 0;
            int start = value.Length > 4 ? value.Length - 4 : 0;
            for (int i = start; i < value.Length; i++)
                result = (result << 8) | value[i];
            return result;
        }

        public static void removeByteArray(ref byte[] array, int start, int length)
        {
            //Console.WriteLine("{0:X}-{1:X}-{2:X}*{3:X}", array.Length, start, length, array.Length - (start + length));
            byte[] newarr = new byte[array.Length - (length)];
            Buffer.BlockCopy(array, 0, newarr, 0, start);
            Buffer.BlockCopy(array, start + length, newarr, start, array.Length - (start + length));
            //newarr = returnportion(array, length, array.Length - length);
            array = newarr;
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            if (NumberChars % 2 != 0)
            {
                hex = "0" + hex;
                NumberChars++;
            }
            if (NumberChars % 4 != 0)
            {
                hex = "00" + hex;
                NumberChars += 2;
            }
            // Substring() allocated a fresh 2-char string for every byte, purely to hand it
            // to Convert.ToByte. Reading the two nibbles directly avoids both the allocation
            // and the parse.
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = (byte)((HexNibble(hex[i]) << 4) | HexNibble(hex[i + 1]));
            return bytes;
        }

        /// <summary>
        /// Value of a single hex digit. Throws on anything else, matching
        /// Convert.ToByte(s, 16), which raised FormatException on invalid input - callers
        /// have try/catch around parsing and rely on that.
        /// </summary>
        private static int HexNibble(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            throw new FormatException("Invalid hex digit: '" + c + "'");
        }

        public static byte[] StringToByteArray_v2(String hex)
        {
            int NumberChars = hex.Length;
            if (NumberChars % 2 != 0)
            {
                hex = "0" + hex;
                NumberChars++;
            }
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        public static byte[] StringToByteArray_v2(String hex, int length)
        {
            int NumberChars = hex.Length;
            if (NumberChars % 2 != 0)
            {
                hex = "0" + hex;
                NumberChars++;
            }
            if (NumberChars % (length * 2) != 0)
            {
                for (int j = NumberChars; j < (length * 2); j += 2) hex = "00" + hex;
                NumberChars = length * 2;
            }
            byte[] bytes = new byte[length];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        public static byte[] StringToByteArray_v3(String hex)
        {
            return Encoding.ASCII.GetBytes(hex);
        }

        public static bool IndexOfSequence(this byte[] buffer, byte[] pattern, int startIndex)
        {
            bool position = false;
            int i = Array.IndexOf<byte>(buffer, pattern[0], startIndex);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual<byte>(pattern))
                    position = true;
                i = Array.IndexOf<byte>(buffer, pattern[0], i + pattern.Length);
            }
            return position;
        }

        public static byte[] TrimStart(byte[] buffer, byte trim)
        {
            int num = 0;
            while (buffer[num] == trim) num++;
            removeByteArray(ref buffer, 0, num);
            return buffer;
        }

        #endregion

        /// <summary>
        /// HMAC - RC4
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        #region crypto stuff

        public static byte[] HMAC_SHA1(byte[] Key, byte[] Message)
        {
            if (Key.Length < 0x10) return null;
            byte[] K = new byte[0x40];
            byte[] opad = new byte[20 + 0x40];
            byte[] ipad = new byte[Message.Length + 0x40];

            Array.Copy(Key, K, 16);

            for (int i = 0; i < 64; i++)
            {
                opad[i] = (byte)(K[i] ^ 0x5C);
                ipad[i] = (byte)(K[i] ^ 0x36);
            }

            // Copy Buffer
            Array.Copy(Message, 0, ipad, 0x40, Message.Length);

            // Get First Hash
            SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
            byte[] Hash1 = sha.ComputeHash(ipad);

            // Copy to OPad
            Array.Copy(Hash1, 0, opad, 0x40, 20);

            return sha.ComputeHash(opad);
        }

        public static void RC4_v(ref Byte[] bytes, Byte[] key)
        {
            // % 256 replaced with & 0xFF throughout. Every operand here is non-negative and
            // bounded (indices 0..255, sums at most 765), and % and & only diverge for
            // negative values in C# - so the substitution is exact, not an approximation.
            // A masked AND is a single instruction where the JIT must emit a division check
            // for %; this runs three times per byte over whole images.
            Byte[] s = new Byte[256];
            Byte temp;
            int i, j;

            // The separate k[] array is gone - it only ever held the key repeated out to 256
            // entries, which is the same as indexing the key modulo its own length.
            int keyLen = key.Length;
            for (i = 0; i < 256; i++) s[i] = (Byte)i;

            j = 0;
            int ki = 0;
            for (i = 0; i < 256; i++)
            {
                j = (j + s[i] + key[ki]) & 0xFF;
                temp = s[i];
                s[i] = s[j];
                s[j] = temp;
                if (++ki == keyLen) ki = 0;
            }

            i = j = 0;
            int len = bytes.Length;   // hoisted: GetLength(0) was re-evaluated every iteration
            for (int x = 0; x < len; x++)
            {
                i = (i + 1) & 0xFF;
                j = (j + s[i]) & 0xFF;
                temp = s[i];
                s[i] = s[j];
                s[j] = temp;
                bytes[x] ^= s[(s[i] + s[j]) & 0xFF];
            }
        }

        #endregion

        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception) { }
            return "";
        }
        public static string GetMD5HashFromFile(byte[] fileName)
        {
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(fileName);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception) { }
            return "";
        }
    }

    public static partial class Extensions
    {
        public static void Endianess(this byte[] src)
        {
            Array.Reverse(src);
        }

        public static bool getBit(this byte src, int bitNumber)
        {
            return (src & (1 << bitNumber)) != 0;
        }

        public static uint toUint(this byte[] src, int offset = 0)
        {
            if (src.Length - offset < 4) return 0;
            byte[] temp = new byte[4];
            Buffer.BlockCopy(src, offset, temp, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(temp);
            }
            return BitConverter.ToUInt32(temp, 0);
        }

        public static void setInt(this byte[] src, int num, int offset = 0)
        {
            byte[] bytes = BitConverter.GetBytes(num);
            Buffer.BlockCopy(bytes, 0, src, offset, bytes.Length);
        }

        public static bool getBit(this byte[] src, int bitNumber)
        {
            return (src.toUint() & (1 << bitNumber)) != 0;
        }

        public static void Replace(this byte[] src, byte[] data, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                src[offset + i] = data[i];
            }
        }

        public static void Fill(this byte[] src, byte fill)
        {
            for (int i = 0; i < src.Length; i++) src[i] = fill;
        }

        public static bool Contains(this byte[] src, byte check)
        {
            for (int i = 0; i < src.Length; i++)
            {
                if (check == src[i]) return true;
            }
            return false;
        }
    }
}
