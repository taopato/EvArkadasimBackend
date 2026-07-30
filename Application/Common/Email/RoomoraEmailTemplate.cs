using System.Net;

namespace Application.Common.Email;

public static class RoomoraEmailTemplate
{
    public static string VerificationCode(string code, string purpose)
    {
        var title = purpose == "reset" ? "Şifreni yenile" : "E-posta adresini doğrula";
        var message = purpose == "reset"
            ? "Roomora şifreni yenilemek için aşağıdaki kodu kullan."
            : "Roomora hesabını tamamlamak için aşağıdaki doğrulama kodunu kullan.";

        return Build(
            title,
            $"""
            <p style="margin:0 0 20px;color:#44546a;font-size:16px;line-height:1.6">{message}</p>
            <div style="margin:22px 0;padding:18px 20px;background:#eaf3fb;border:1px solid #c7ddef;border-radius:10px;text-align:center">
              <div style="color:#24567f;font-size:13px;font-weight:700;text-transform:uppercase">Doğrulama kodu</div>
              <div style="margin-top:8px;color:#18344d;font-size:34px;font-weight:800;letter-spacing:6px">{WebUtility.HtmlEncode(code)}</div>
            </div>
            <p style="margin:0;color:#66768a;font-size:14px;line-height:1.6">Kod 10 dakika geçerlidir. Bu işlemi sen başlatmadıysan bu e-postayı yok sayabilirsin.</p>
            """);
    }

    public static string Invitation(string inviteLink, DateTime expiresAt)
    {
        var safeLink = WebUtility.HtmlEncode(inviteLink);
        return Build(
            "Eve davet edildin",
            $"""
            <p style="margin:0 0 12px;color:#22354a;font-size:17px;font-weight:700">Ortak yaşam şimdi daha kolay.</p>
            <p style="margin:0 0 24px;color:#44546a;font-size:16px;line-height:1.6">Bir ev arkadaşı seni Roomora'daki evine davet etti. Daveti kabul ederek ortak giderleri, faturaları ve notları birlikte yönetebilirsin.</p>
            <div style="text-align:center;margin:28px 0">
              <a href="{safeLink}" style="display:inline-block;padding:13px 24px;background:#2f6fa8;color:#fff;text-decoration:none;border-radius:8px;font-size:16px;font-weight:700">Daveti Kabul Et</a>
            </div>
            <p style="margin:0;color:#66768a;font-size:13px;line-height:1.6">Bağlantı {expiresAt:dd.MM.yyyy} tarihine kadar geçerlidir.</p>
            <p style="margin:12px 0 0;color:#66768a;font-size:12px;line-height:1.5;word-break:break-all">Buton açılmazsa: <a href="{safeLink}" style="color:#2f6fa8">{safeLink}</a></p>
            """);
    }

    public static string AccountDeletion(string deletionUrl)
    {
        var safeLink = WebUtility.HtmlEncode(deletionUrl);
        return Build(
            "Hesap silme isteği",
            $"""
            <p style="margin:0 0 20px;color:#44546a;font-size:16px;line-height:1.6">Roomora hesabını kalıcı olarak silme isteği aldık.</p>
            <div style="text-align:center;margin:28px 0">
              <a href="{safeLink}" style="display:inline-block;padding:13px 24px;background:#b42318;color:#fff;text-decoration:none;border-radius:8px;font-size:16px;font-weight:700">Hesabımı Kalıcı Olarak Sil</a>
            </div>
            <p style="margin:0;color:#66768a;font-size:14px;line-height:1.6">Bu bağlantı 15 dakika geçerlidir. İsteği sen yapmadıysan e-postayı yok sayabilirsin.</p>
            """);
    }

    private static string Build(string title, string content)
    {
        return $"""
        <!doctype html>
        <html lang="tr">
        <head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
        <body style="margin:0;padding:0;background:#f7f9fc;font-family:Arial,Helvetica,sans-serif">
          <div style="display:none;max-height:0;overflow:hidden">{WebUtility.HtmlEncode(title)} - Roomora</div>
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f7f9fc">
            <tr><td align="center" style="padding:28px 14px">
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:560px;background:#fff;border:1px solid #dfe7ef;border-radius:12px;overflow:hidden">
                <tr><td style="padding:24px 28px;background:#18344d;color:#fff">
                  <div style="font-size:25px;font-weight:800">Roomora</div>
                  <div style="margin-top:4px;color:#c7ddef;font-size:13px">Ortak yaşamın kolay hali</div>
                </td></tr>
                <tr><td style="padding:30px 28px">
                  <h1 style="margin:0 0 18px;color:#18344d;font-size:25px;line-height:1.25">{WebUtility.HtmlEncode(title)}</h1>
                  {content}
                </td></tr>
                <tr><td style="padding:18px 28px;border-top:1px solid #e8edf3;color:#7a8797;font-size:12px">Roomora · Takosware</td></tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
    }
}
