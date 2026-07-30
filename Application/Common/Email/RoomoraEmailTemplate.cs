using System.Net;

namespace Application.Common.Email;

public static class RoomoraEmailTemplate
{
    public static string VerificationCode(string code, string purpose)
    {
        var isPasswordReset = purpose == "reset";
        var title = isPasswordReset ? "Şifreni yenile" : "E-posta adresini doğrula";
        var eyebrow = isPasswordReset ? "Güvenli hesap erişimi" : "Roomora'ya hoş geldin";
        var message = isPasswordReset
            ? "Roomora şifreni yenilemek için aşağıdaki tek kullanımlık kodu kullan."
            : "Hesabını tamamlamak için aşağıdaki doğrulama kodunu Roomora uygulamasına gir.";

        return Build(
            title,
            eyebrow,
            $"""
            <p style="margin:0 0 24px;color:#526579;font-size:16px;line-height:1.65">{message}</p>
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="margin:0 0 22px;background:#edf5fc;border:1px solid #cfe1f1;border-radius:14px">
              <tr>
                <td align="center" style="padding:22px 18px">
                  <div style="color:#2f6fa8;font-size:12px;font-weight:700;text-transform:uppercase">Doğrulama kodun</div>
                  <div style="margin-top:9px;color:#15324b;font-size:36px;line-height:1.1;font-weight:800;letter-spacing:7px">{WebUtility.HtmlEncode(code)}</div>
                </td>
              </tr>
            </table>
            {InfoBox("10 dakika geçerli", "Güvenliğin için bu kodu kimseyle paylaşma. Bu işlemi sen başlatmadıysan e-postayı yok sayabilirsin.")}
            """);
    }

    public static string Invitation(string inviteLink, DateTime expiresAt)
    {
        var safeLink = WebUtility.HtmlEncode(inviteLink);
        return Build(
            "Eve davet edildin",
            "Birlikte yaşamak artık daha kolay",
            $"""
            <p style="margin:0 0 14px;color:#243b52;font-size:17px;font-weight:700">Roomora'da ev arkadaşların seni bekliyor.</p>
            <p style="margin:0 0 26px;color:#526579;font-size:16px;line-height:1.65">Daveti kabul ederek ortak giderleri, faturaları ve ev notlarını tek bir yerde birlikte yönetebilirsin.</p>
            {PrimaryButton(safeLink, "Daveti Kabul Et")}
            {InfoBox($"{expiresAt:dd.MM.yyyy} tarihine kadar geçerli", "Butona dokunduğunda Roomora uygulaması açılır ve davet bilgilerin güvenli şekilde aktarılır.")}
            <p style="margin:18px 0 0;color:#7b8998;font-size:12px;line-height:1.55;word-break:break-all">Buton açılmazsa bu bağlantıyı kullan:<br><a href="{safeLink}" style="color:#2f6fa8;text-decoration:underline">{safeLink}</a></p>
            """);
    }

    public static string AccountDeletion(string deletionUrl)
    {
        var safeLink = WebUtility.HtmlEncode(deletionUrl);
        return Build(
            "Hesap silme isteği",
            "Önemli güvenlik bildirimi",
            $"""
            <p style="margin:0 0 24px;color:#526579;font-size:16px;line-height:1.65">Roomora hesabını kalıcı olarak silmek için bir istek aldık. Bu işlem ev ve hesap verilerini geri alınamayacak şekilde kaldırır.</p>
            {DangerButton(safeLink, "Hesabımı Kalıcı Olarak Sil")}
            {InfoBox("15 dakika geçerli", "Bu isteği sen yapmadıysan hiçbir işlem yapma. Hesabın güvende kalmaya devam eder.")}
            """);
    }

    private static string Build(string title, string eyebrow, string content)
    {
        var safeTitle = WebUtility.HtmlEncode(title);
        var safeEyebrow = WebUtility.HtmlEncode(eyebrow);

        return $"""
        <!doctype html>
        <html lang="tr">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width,initial-scale=1">
          <meta name="color-scheme" content="light">
          <meta name="supported-color-schemes" content="light">
          <title>{safeTitle}</title>
        </head>
        <body style="margin:0;padding:0;background:#f3f7fb;font-family:Arial,'Helvetica Neue',Helvetica,sans-serif;-webkit-text-size-adjust:100%">
          <div style="display:none;max-height:0;overflow:hidden;opacity:0;color:transparent">{safeTitle} · Roomora</div>
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="width:100%;background:#f3f7fb">
            <tr>
              <td align="center" style="padding:32px 14px">
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="width:100%;max-width:580px;background:#ffffff;border:1px solid #dce7f0;border-radius:16px;overflow:hidden">
                  <tr>
                    <td style="height:5px;background:#2f6fa8;font-size:0;line-height:0">&nbsp;</td>
                  </tr>
                  <tr>
                    <td style="padding:26px 30px 20px">
                      <table role="presentation" cellspacing="0" cellpadding="0">
                        <tr>
                          <td width="46" height="46" align="center" valign="middle" style="width:46px;height:46px;background:#e7f1fa;border:1px solid #c8dff1;border-radius:13px;color:#1d537f;font-size:22px;font-weight:800">R</td>
                          <td style="padding-left:13px">
                            <div style="color:#173550;font-size:22px;line-height:1.1;font-weight:800">Roomora</div>
                            <div style="margin-top:4px;color:#668096;font-size:12px">Ortak yaşamın kolay hali</div>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                  <tr>
                    <td style="padding:10px 30px 32px">
                      <div style="margin-bottom:9px;color:#2f6fa8;font-size:12px;font-weight:700;text-transform:uppercase">{safeEyebrow}</div>
                      <h1 style="margin:0 0 18px;color:#173550;font-size:28px;line-height:1.25;font-weight:800">{safeTitle}</h1>
                      {content}
                    </td>
                  </tr>
                  <tr>
                    <td style="padding:20px 30px;background:#f8fafc;border-top:1px solid #e4ebf2">
                      <div style="color:#718194;font-size:12px;line-height:1.6">Bu e-posta Roomora işleminle ilgili olarak otomatik gönderildi.</div>
                      <div style="margin-top:6px;color:#8b98a6;font-size:11px">Roomora · Takosware</div>
                    </td>
                  </tr>
                </table>
                <div style="padding:18px 12px 0;color:#8795a4;font-size:11px;line-height:1.5">© {DateTime.UtcNow.Year} Takosware. Tüm hakları saklıdır.</div>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;
    }

    private static string PrimaryButton(string safeUrl, string label) =>
        $"""
        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="margin:0 0 22px">
          <tr><td align="center">
            <a href="{safeUrl}" style="display:inline-block;min-width:210px;padding:14px 24px;background:#2f6fa8;color:#ffffff;text-decoration:none;border-radius:10px;font-size:16px;font-weight:700;text-align:center">{label}</a>
          </td></tr>
        </table>
        """;

    private static string DangerButton(string safeUrl, string label) =>
        $"""
        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="margin:0 0 22px">
          <tr><td align="center">
            <a href="{safeUrl}" style="display:inline-block;min-width:240px;padding:14px 24px;background:#b42318;color:#ffffff;text-decoration:none;border-radius:10px;font-size:15px;font-weight:700;text-align:center">{label}</a>
          </td></tr>
        </table>
        """;

    private static string InfoBox(string title, string message) =>
        $"""
        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f7f9fc;border:1px solid #e1e8ef;border-radius:10px">
          <tr>
            <td style="padding:15px 17px">
              <div style="color:#344b61;font-size:13px;font-weight:700">{WebUtility.HtmlEncode(title)}</div>
              <div style="margin-top:5px;color:#718194;font-size:13px;line-height:1.55">{WebUtility.HtmlEncode(message)}</div>
            </td>
          </tr>
        </table>
        """;
}
