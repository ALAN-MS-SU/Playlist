using OtpNet;
using Microsoft.AspNetCore.DataProtection;
namespace CaixaAPI.Model.TOPT;

public class TOPT
{
   public string Issuer;
   
   private int Length;

   private IDataProtector Protector;

   public TOPT(IConfiguration Configuration, IDataProtectionProvider Provider)
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
   
}