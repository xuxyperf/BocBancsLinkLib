//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace EbcdicEncoding
//{
//    public class Class1
//    {
//    }
//}
using System;
using System.Collections;
using System.Text;
using System.Reflection;
using System.IO;
using System.Globalization;

public class EbcdicEncoding : Encoding
{
    // Fields
    private static string[] allNames = new string[0];
    private readonly char[] byteToCharMap;
    private readonly byte[][] charBlockToByteBlockMap = new byte[0x100][];
    private static readonly IDictionary encodingMap = new Hashtable();
    private string name;
    private byte unknownCharacterByte;

    // Methods
    public  EbcdicEncoding()
    {
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("IBM1388Encoding.ebcdic.dat"))
        {
            if (stream == null)
            {
                throw new InvalidEbcdicDataException("EBCDIC encodings resource not found.");
            }
            int num = stream.ReadByte();
            if (num == -1)
            {
                throw new InvalidEbcdicDataException("EBCDIC encodings resource empty.");
            }
            allNames = new string[num];
            for (int i = 0; i < num; i++)
            {
                int count = stream.ReadByte();
                if (count == -1)
                {
                    throw new InvalidEbcdicDataException("EBCDIC encodings resource truncated.");
                }
                string name = "EBCDIC-" + Encoding.ASCII.GetString(ReadFully(stream, count), 0, count);
                allNames[i] = name;
                byte[] buffer = ReadFully(stream, 0x200);
                char[] byteToCharMap = new char[0x100];
                for (int j = 0; j < 0x100; j++)
                {
                    byteToCharMap[j] = (char)((buffer[j * 2] << 8) | buffer[(j * 2) + 1]);
                }
                encodingMap[name.ToUpper(CultureInfo.InvariantCulture)] = new EbcdicEncoding(name, byteToCharMap);
            }
            if (stream.ReadByte() != -1)
            {
                throw new InvalidEbcdicDataException("EBCDIC encodings resource contains unused data.");
            }
        }
    }

    public EbcdicEncoding(string name, char[] byteToCharMap)
    {
        this.name = name;
        this.byteToCharMap = byteToCharMap;
        this.ConstructCharToByteMaps();
        this.unknownCharacterByte = this.Encode('?');
    }

    private void ConstructCharToByteMaps()
    {
        for (int i = 0; i < 0x100; i++)
        {
            char ch = this.byteToCharMap[i];
            byte[] buffer = this.charBlockToByteBlockMap[ch >> 8];
            if (buffer == null)
            {
                buffer = new byte[0x100];
                this.charBlockToByteBlockMap[ch >> 8] = buffer;
            }
            buffer[ch & '\x00ff'] = (byte)i;
        }
    }

    private char Decode(byte byteValue)
    {
        char ch = this.byteToCharMap[byteValue];
        if ((ch == '\0') && (byteValue != 0))
        {
            ch = '?';
        }
        return ch;
    }

    private byte Encode(char character)
    {
        byte num;
        byte[] buffer = this.charBlockToByteBlockMap[character >> 8];
        if (buffer == null)
        {
            num = 0;
        }
        else
        {
            num = buffer[character & '\x00ff'];
        }
        if ((num == 0) && (character != '\0'))
        {
            return this.unknownCharacterByte;
        }
        return num;
    }

    public override int GetByteCount(char[] chars, int index, int count)
    {
        ValidateParameters(chars, index, count, "GetByteCount");
        return count;
    }

    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
        ValidateParameters(chars, charIndex, charCount, "GetBytes");
        ValidateParameters(bytes, byteIndex, 0, "GetBytes");
        if ((byteIndex + charCount) > bytes.Length)
        {
            throw new ArgumentException("Byte array passed to GetBytes is too short");
        }
        for (int i = 0; i < charCount; i++)
        {
            bytes[byteIndex + i] = this.Encode(chars[charIndex + i]);
        }
        return charCount;
    }

    public override int GetCharCount(byte[] bytes, int index, int count)
    {
        ValidateParameters(bytes, index, count, "GetCharCount");
        return count;
    }

    public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
    {
        ValidateParameters(bytes, byteIndex, byteCount, "GetChars");
        ValidateParameters(chars, charIndex, 0, "GetChars");
        if ((charIndex + byteCount) > chars.Length)
        {
            throw new ArgumentException("Character array passed to GetChars is too short");
        }
        for (int i = 0; i < byteCount; i++)
        {
            chars[charIndex + i] = this.Decode(bytes[byteIndex + i]);
        }
        return byteCount;
    }

    public new static Encoding GetEncoding(string name)
    {
        EbcdicEncoding encoding = (EbcdicEncoding)encodingMap[name.ToUpper(CultureInfo.InvariantCulture)];
        if (encoding == null)
        {
            throw new NotSupportedException("No EBCDIC encoding named " + name + " found.");
        }
        return encoding;
    }

    public override int GetMaxByteCount(int charCount)
    {
        return charCount;
    }

    public override int GetMaxCharCount(int byteCount)
    {
        return byteCount;
    }

    private static byte[] ReadFully(Stream stream, int count)
    {
        int num2;
        byte[] buffer = new byte[count];
        for (int i = 0; i < count; i += num2)
        {
            num2 = stream.Read(buffer, i, count - i);
            if (num2 <= 0)
            {
                throw new InvalidEbcdicDataException("EBCDIC encodings resource truncated.");
            }
        }
        return buffer;
    }

    private static void ValidateParameters(Array array, int index, int count, string methodName)
    {
        if (array == null)
        {
            throw new ArgumentNullException("Null array passed to " + methodName);
        }
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException("Negative index passed to " + methodName);
        }
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException("Negative count passed to " + methodName);
        }
        if ((index + count) > array.Length)
        {
            throw new ArgumentOutOfRangeException("index+count > length in " + methodName);
        }
    }

    // Properties
    public static string[] AllNames
    {
        get
        {
            return (string[])allNames.Clone();
        }
    }

    public string Name
    {
        get
        {
            return this.name;
        }
    }
}

internal class InvalidEbcdicDataException : Exception
{
    // Methods
    internal InvalidEbcdicDataException(string reason)
        : base(reason)
    {
    }
}