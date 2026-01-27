using Microsoft.AspNetCore.DataProtection;
using OtpNet;
using QRCoder;
using StackExchange.Redis;
namespace CaixaAPI.Model.TOTP;

public class TOTP
{
    public string Issuer;

    private readonly int Length;

    private readonly IDataProtector Protector;

    private readonly IDatabase Redis;

    private readonly string Prefix;
    
    private readonly int Timeout;

    public TOTP(IConfiguration Configuration, IDataProtectionProvider Provider, IConnectionMultiplexer Redis)
    {
        this.Protector = Provider.CreateProtector("TOPT-Encrypt");
        this.Length = int.Parse(Configuration["TOPT:Length"]!);
        this.Issuer = Configuration["TOPT:Issuer"]!;
        this.Redis = Redis.GetDatabase();
        this.Prefix = Configuration["TOPT:Prefix"]!;
        this.Timeout = int.Parse(Configuration["TOPT:Timeout"]!);
    }
    public async Task<long> Count(string Email)
    {
        var Count = await this.Redis.ListLengthAsync($"{this.Prefix}-{Email}");
        return Count;
    }

    public void Attempt(string Email)
    {
         this.Redis.ListRightPushAsync($"{this.Prefix}-{Email}", $"");
         this.Redis.KeyExpireAsync($"{this.Prefix}-{Email}", TimeSpan.FromHours(this.Timeout));
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