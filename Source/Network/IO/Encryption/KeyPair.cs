using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Mirage.Sharp.Asfw.IO.Encryption
{
  public sealed class KeyPair : IDisposable
  {
    private RSACryptoServiceProvider _rsa;

    public void Dispose()
    {
      this._rsa.Dispose();
      this._rsa = (RSACryptoServiceProvider) null;
    }

    public bool PublicOnly => this._rsa != null ? this._rsa.PublicOnly : throw new CryptographicException("Key(s) not found!");

    public void GenerateKeys() => this._rsa = new RSACryptoServiceProvider(2048);

    public string ExportKeyString(bool exportPrivate = false) => this._rsa.ToXmlString(exportPrivate);

    public void ExportKey(string file, bool exportPrivate = true)
    {
      StreamWriter streamWriter = new StreamWriter(file, false);
      streamWriter.Write(this._rsa.ToXmlString(exportPrivate));
      streamWriter.Close();
    }

    public void ImportKeyString(string key)
    {
      this._rsa = new RSACryptoServiceProvider();
      this._rsa.FromXmlString(key);
    }

    public void ImportKey(string file)
    {
      StreamReader streamReader = new StreamReader(file);
      this._rsa = new RSACryptoServiceProvider();
      this._rsa.FromXmlString(streamReader.ReadToEnd());
      streamReader.Close();
    }

    public byte[] EncryptBytes(byte[] value)
    {
      if (this._rsa == null)
        throw new CryptographicException("Key not set.");
      var rijndaelManaged1 = Aes.Create("AesManaged");
      rijndaelManaged1.KeySize = 256;
      rijndaelManaged1.BlockSize = 128;
      rijndaelManaged1.Mode = CipherMode.CBC;
      var rijndaelManaged2 = rijndaelManaged1;
      using (MemoryStream memoryStream = new MemoryStream())
      {
        memoryStream.Write(this._rsa?.Encrypt(rijndaelManaged2?.Key, false), 0, 256);
        memoryStream.Write(rijndaelManaged2.IV, 0, 16);
        using (ICryptoTransform encryptor = rijndaelManaged2.CreateEncryptor())
        {
          using (CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, encryptor, CryptoStreamMode.Write))
          {
            cryptoStream.Write(value, 0, value.Length);
            cryptoStream.FlushFinalBlock();
          }
        }
        return memoryStream.ToArray();
      }
    }

    public async Task<byte[]> EncryptBytesAsync(byte[] value)
    {
      if (this._rsa == null)
        throw new CryptographicException("Key not set.");
      var rijndaelManaged = Aes.Create("AesManaged");
      rijndaelManaged.KeySize = 256;
      rijndaelManaged.BlockSize = 128;
      rijndaelManaged.Mode = CipherMode.CBC;
      var rm = rijndaelManaged;
      byte[] array;
      using (MemoryStream ms = new MemoryStream())
      {
        await ms.WriteAsync(this._rsa.Encrypt(rm.Key, false), 0, 256);
        await ms.WriteAsync(rm.IV, 0, 16);
        using (ICryptoTransform es = rm.CreateEncryptor())
        {
          using (CryptoStream cs = new CryptoStream((Stream) ms, es, CryptoStreamMode.Write))
          {
            await cs.WriteAsync(value, 0, value.Length);
            cs.FlushFinalBlock();
          }
        }
        array = ms.ToArray();
      }
      rm = null;
      return array;
    }

    public byte[] EncryptBytes(byte[] value, int offset, int size)
    {
      if (this._rsa == null)
        throw new CryptographicException("Key not set.");
      var rijndaelManaged1 = Aes.Create("AesManaged");
      rijndaelManaged1.KeySize = 256;
      rijndaelManaged1.BlockSize = 128;
      rijndaelManaged1.Mode = CipherMode.CBC;
      var rijndaelManaged2 = rijndaelManaged1;
      using (ICryptoTransform encryptor = rijndaelManaged2.CreateEncryptor())
      {
        using (MemoryStream memoryStream = new MemoryStream())
        {
          memoryStream.Write(this._rsa.Encrypt(rijndaelManaged2.Key, false), 0, 256);
          memoryStream.Write(rijndaelManaged2.IV, 0, 16);
          using (CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, encryptor, CryptoStreamMode.Write))
          {
            cryptoStream.Write(value, offset, size);
            cryptoStream.FlushFinalBlock();
          }
          return memoryStream.ToArray();
        }
      }
    }

    public async Task<byte[]> EncryptBytesAsync(byte[] value, int offset, int size)
    {
      if (this._rsa == null)
        throw new CryptographicException("Key not set.");
      var rijndaelManaged = Aes.Create("AesManaged");
      rijndaelManaged.KeySize = 256;
      rijndaelManaged.BlockSize = 128;
      rijndaelManaged.Mode = CipherMode.CBC;
      var rm = rijndaelManaged;
      byte[] array;
      using (ICryptoTransform es = rm.CreateEncryptor())
      {
        using (MemoryStream ms = new MemoryStream())
        {
          await ms.WriteAsync(this._rsa.Encrypt(rm.Key, false), 0, 256);
          await ms.WriteAsync(rm.IV, 0, 16);
          using (CryptoStream cs = new CryptoStream((Stream) ms, es, CryptoStreamMode.Write))
          {
            await cs.WriteAsync(value, offset, size);
            cs.FlushFinalBlock();
          }
          array = ms.ToArray();
        }
      }
      rm = null;
      return array;
    }

    public string EncryptString(string value)
    {
      if (this._rsa == null)
      {
        return "";
      }
      
      return Convert.ToBase64String(this.EncryptBytes(Encoding.UTF8.GetBytes(value)));
    }

    public async Task<string> EncryptStringAsync(string value)
    {
      if (this._rsa == null)
        throw new CryptographicException("Key not set.");
      return Convert.ToBase64String(await this.EncryptBytesAsync(Encoding.UTF8.GetBytes(value)));
    }

    public void EncryptFile(string srcFile, string dstFile)
    {
      if (this._rsa == null)
        throw new CryptographicException("Key not set.");
      File.WriteAllBytes(dstFile, this.EncryptBytes(File.ReadAllBytes(srcFile)));
    }

    public async Task EncryptFileAsync(string srcFile, string dstFile)
    {
      if (this._rsa == null)
        throw new CryptographicException("Key not set.");
      string path = dstFile;
      File.WriteAllBytes(path, await this.EncryptBytesAsync(File.ReadAllBytes(srcFile)));
      path = (string) null;
    }

    public byte[] DecryptBytes(byte[] value)
    {
      if (this._rsa == null)
        throw new CryptographicException("Key not set.");
      if (this._rsa.PublicOnly)
        return (byte[]) null;
      if (value.Length < 272)
        return (byte[]) null;
      var rijndaelManaged = Aes.Create("AesManaged");
      rijndaelManaged.KeySize = 256;
      rijndaelManaged.BlockSize = 128;
      rijndaelManaged.Mode = CipherMode.CBC;
      byte[] numArray1 = new byte[256];
      byte[] numArray2 = new byte[16];
      int count = value.Length - 272;
      byte[] numArray3 = new byte[count];
      Buffer.BlockCopy((Array) value, 0, (Array) numArray1, 0, 256);
      Buffer.BlockCopy((Array) value, 256, (Array) numArray2, 0, 16);
      Buffer.BlockCopy((Array) value, 272, (Array) numArray3, 0, count);
      using (ICryptoTransform decryptor = rijndaelManaged.CreateDecryptor(this._rsa.Decrypt(numArray1, false), numArray2))
      {
        using (MemoryStream memoryStream = new MemoryStream())
        {
          using (CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, decryptor, CryptoStreamMode.Write))
          {
            cryptoStream.Write(numArray3, 0, count);
            cryptoStream.FlushFinalBlock();
            return memoryStream.ToArray();
          }
        }
      }
    }

    public async Task<byte[]> DecryptBytesAsync(byte[] value)
    {
      if (this._rsa == null)
        throw new CryptographicException("Key not set.");
      if (this._rsa.PublicOnly || value.Length < 272)
        return (byte[]) null;
      var rijndaelManaged1 = Aes.Create("AesManaged");
      rijndaelManaged1.KeySize = 256;
      rijndaelManaged1.BlockSize = 128;
      rijndaelManaged1.Mode = CipherMode.CBC;
      var rijndaelManaged2 = rijndaelManaged1;
      byte[] numArray1 = new byte[256];
      byte[] numArray2 = new byte[16];
      int count = value.Length - 272;
      byte[] numArray3 = new byte[count];
      Buffer.BlockCopy((Array) value, 0, (Array) numArray1, 0, 256);
      Buffer.BlockCopy((Array) value, 256, (Array) numArray2, 0, 16);
      Buffer.BlockCopy((Array) value, 272, (Array) numArray3, 0, count);
      using (ICryptoTransform ds = rijndaelManaged2.CreateDecryptor(this._rsa.Decrypt(numArray1, false), numArray2))
      {
        using (MemoryStream ms = new MemoryStream())
        {
          using (CryptoStream cs = new CryptoStream((Stream) ms, ds, CryptoStreamMode.Write))
          {
            await cs.WriteAsync(numArray3, 0, count);
            cs.FlushFinalBlock();
            return ms.ToArray();
          }
        }
      }
    }

    public byte[] DecryptBytes(byte[] value, int offset, int size)
    {
      if (this._rsa == null)
        throw new CryptographicException("Key not set.");
      if (this._rsa.PublicOnly)
        return (byte[]) null;
      if (value.Length < 272)
        return (byte[]) null;
      if (value.Length < offset + size)
        return (byte[]) null;
      var rijndaelManaged = Aes.Create("AesManaged");
      rijndaelManaged.KeySize = 256;
      rijndaelManaged.BlockSize = 128;
      rijndaelManaged.Mode = CipherMode.CBC;
      byte[] numArray1 = new byte[256];
      byte[] numArray2 = new byte[16];
      int count = size - 272;
      byte[] numArray3 = new byte[count];
      Buffer.BlockCopy((Array) value, offset, (Array) numArray1, 0, 256);
      Buffer.BlockCopy((Array) value, offset + 256, (Array) numArray2, 0, 16);
      Buffer.BlockCopy((Array) value, offset + 272, (Array) numArray3, 0, count);
      using (ICryptoTransform decryptor = rijndaelManaged.CreateDecryptor(this._rsa.Decrypt(numArray1, false), numArray2))
      {
        using (MemoryStream memoryStream = new MemoryStream())
        {
          using (CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, decryptor, CryptoStreamMode.Write))
          {
            cryptoStream.Write(numArray3, 0, count);
            cryptoStream.FlushFinalBlock();
            return memoryStream.ToArray();
          }
        }
      }
    }

    public async Task<byte[]> DecryptBytesAsync(byte[] value, int offset, int size)
    {
      if (this._rsa == null)
        throw new CryptographicException("Key not set.");
      if (this._rsa.PublicOnly || value.Length < 272 || value.Length < offset + size)
        return (byte[]) null;
      var rijndaelManaged1 = Aes.Create("AesManaged");
      rijndaelManaged1.KeySize = 256;
      rijndaelManaged1.BlockSize = 128;
      rijndaelManaged1.Mode = CipherMode.CBC;
      var rijndaelManaged2 = rijndaelManaged1;
      byte[] numArray1 = new byte[256];
      byte[] numArray2 = new byte[16];
      int count = size - 272;
      byte[] numArray3 = new byte[count];
      Buffer.BlockCopy((Array) value, offset, (Array) numArray1, 0, 256);
      Buffer.BlockCopy((Array) value, offset + 256, (Array) numArray2, 0, 16);
      Buffer.BlockCopy((Array) value, offset + 272, (Array) numArray3, 0, count);
      using (ICryptoTransform ds = rijndaelManaged2.CreateDecryptor(this._rsa.Decrypt(numArray1, false), numArray2))
      {
        using (MemoryStream ms = new MemoryStream())
        {
          using (CryptoStream cs = new CryptoStream((Stream) ms, ds, CryptoStreamMode.Write))
          {
            await cs.WriteAsync(numArray3, 0, count);
            cs.FlushFinalBlock();
            return ms.ToArray();
          }
        }
      }
    }

    public string DecryptString(string value)
    {
      if (this._rsa == null)
        throw new CryptographicException("Key not set.");
      if (this._rsa.PublicOnly)
        return (string) null;
      byte[] numArray = Convert.FromBase64String(value);
      return numArray.Length >= 272 ? Encoding.UTF8.GetString(this.DecryptBytes(numArray)) : "";
    }

    public async Task<string> DecryptStringAsync(string value)
    {
      if (this._rsa == null)
        throw new CryptographicException("Key not set.");
      if (this._rsa.PublicOnly)
        return (string) null;
      byte[] numArray = Convert.FromBase64String(value);
      string str;
      if (numArray.Length < 272)
      {
        str = "";
      }
      else
      {
        Encoding encoding = Encoding.UTF8;
        str = encoding.GetString(await this.DecryptBytesAsync(numArray));
        encoding = (Encoding) null;
      }
      return str;
    }

    public void DecryptFile(string srcFile, string dstFile)
    {
      if (this._rsa == null)
        throw new CryptographicException("Key not set.");
      if (this._rsa.PublicOnly)
        return;
      File.WriteAllBytes(dstFile, this.DecryptBytes(File.ReadAllBytes(srcFile)) ?? throw new FileNotFoundException("File contents were empty!"));
    }

    public async Task DecryptFileAsync(string srcFile, string dstFile)
    {
      if (this._rsa == null)
        throw new CryptographicException("Key not set.");
      if (this._rsa.PublicOnly)
        return;
      File.WriteAllBytes(dstFile, await this.DecryptBytesAsync(File.ReadAllBytes(srcFile)) ?? throw new FileNotFoundException("File contents were empty!"));
    }

    public enum KeyType
    {
      Signature = 1,
      Exchange = 2,
    }
  }
}
