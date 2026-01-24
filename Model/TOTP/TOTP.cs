using OtpNet;
using Microsoft.AspNetCore.DataProtection;
using QRCoder;

namespace CaixaAPI.Model.TOTP;

public class TOTP
{
   public string Issuer;
   
   private int Length;

   private IDataProtector Protector;

   public TOTP(IConfiguration Configuration, IDataProtectionProvider Provider)
   {
      
      this.Protector = Provider.CreateProtector("TOPT-Encrypt");
      this.Length = Int32.Parse(Configuration["TOPT:Length"]!);
      this.Issuer = Configuration["TOPT:Issuer"]!;
   }

 
   public (string Secret, string Encrypt) CreateSecret()
   {
      if(this.Length == null || this.Issuer == null) throw new InvalidOperationException("Config Length not found.");
      var Key = KeyGeneration.GenerateRandomKey(this.Length);
      string Secret = Base32Encoding.ToString(Key);
      var Encrypt = Protector.Protect(Secret);
      return (Secret,Encrypt);
   }

   public byte[] GenerateQRCode(string Email, string Secret, string Issuer)
   {
      var QRGenerator = new QRCodeGenerator();
      var QRData = QRGenerator.CreateQrCode
      ($"otpauth://totp/PlaylistApp:{Email}?secret={Protector.Unprotect(Secret)}&issuer={Issuer}"
         , QRCodeGenerator.ECCLevel.Q);
      return new PngByteQRCode(QRData).GetGraphic(20);
   }

   public bool Valid(string Secret, string Code)
   {
     var Decrypt = Protector.Unprotect(Secret);
     
     
     
     var Key = Base32Encoding.ToBytes(Decrypt);

     var TOTP = new Totp(Key);
     
     return TOTP.VerifyTotp(Code,out long Step, new VerificationWindow(previous:0,future:0));
   }
}

public class TOPTCode
{
   public string Code { get; set; }
   
   public string Email  { get; set; }
   public TOPTCode(string Code, string Email)
   {
      this.Code = Code;
      this.Email = Email;
   }
}