using Microsoft.AspNetCore.DataProtection;
using OtpNet;
using QRCoder;
namespace CaixaAPI.Model.TOTP;

public class TOTP
{
    public string Issuer;

    private readonly int Length;

    private readonly IDataProtector Protector;
    

    public TOTP(IConfiguration Configuration, IDataProtectionProvider Provider)
    {
        this.Protector = Provider.CreateProtector("TOTP-Encrypt");
        this.Length = int.Parse(Configuration["TOTP:Length"]!);
        this.Issuer = Configuration["TOTP:Issuer"]!;
        
    }
    public (string Secret, string Encrypt) CreateSecret()
    {
        if (Length == null || Issuer == null) throw new InvalidOperationException("Config Length not found.");
        var Key = KeyGeneration.GenerateRandomKey(Length);
        var Secret = Base32Encoding.ToString(Key);
        var Encrypt = Protector.Protect(Secret);
        return (Secret, Encrypt);
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

        return TOTP.VerifyTotp(Code, out var Step, new VerificationWindow());
    }
}
public class TOPTCode
{
    public TOPTCode(string Code, string Email)
    {
        this.Code = Code;
        this.Email = Email;
    }

    public string Code { get; set; }

    public string Email { get; set; }
}