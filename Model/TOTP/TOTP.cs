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

public class Access
{
        protected readonly IDatabase Redis;
        protected readonly int Timeout;
        public Access(IConfiguration Configuration,IConnectionMultiplexer Redis)
        {
            this.Redis = Redis.GetDatabase();
            this.Timeout = int.Parse(Configuration["TOTP:Timeout"]!);
        }
        public void Count(string Email)
        {
            
        }
        public void Attempt(string Email)
        {
            
        }
}

public class  TFAccess : Access
{
    private readonly string TFPrefix;

    public TFAccess(IConfiguration Configuration, IConnectionMultiplexer Redis) : base(Configuration, Redis)
    {
        this.TFPrefix = Configuration["TOTP:TFPrefix"]!;
    }
    public async Task<long> Count(string Email)
    {
        var Count = await this.Redis.ListLengthAsync($"{this.TFPrefix}-{Email}");
        return Count;
    }
    public void Attempt(string Email)
    {
        this.Redis.ListRightPushAsync($"{this.TFPrefix}-{Email}", $"");
        this.Redis.KeyExpireAsync($"{this.TFPrefix}-{Email}", TimeSpan.FromMinutes(this.Timeout));
    }
}
public class  SIAccess : Access
{
    private readonly string SIPrefix;
    public SIAccess(IConfiguration Configuration, IConnectionMultiplexer Redis) : base(Configuration, Redis)
    {
        this.SIPrefix = Configuration["TOTP:SIPrefix"]!;
    }
    public async Task<long> Count(string Email)
    {
        var Count = await this.Redis.ListLengthAsync($"{this.SIPrefix}-{Email}");
        return Count;
    }
    public void Attempt(string Email)
    {
        this.Redis.ListRightPushAsync($"{this.SIPrefix}-{Email}", $"");
        this.Redis.KeyExpireAsync($"{this.SIPrefix}-{Email}", TimeSpan.FromHours(this.Timeout));
    }

    public  void Remove(string Email)
    {
        this.Redis.KeyDelete($"{this.SIPrefix}-{Email}");
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