<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <title>Şifre Yenileme Talebi</title>
</head>
<body style="font-family: Arial, Helvetica, sans-serif; background-color: #f4f6f8; padding: 20px; margin: 0;">

    <table width="100%" cellpadding="0" cellspacing="0" style="max-width: 600px; margin: auto; background: white; border-radius: 8px; padding: 30px;">

        <tr>
            <td style="text-align: center;">
                <h2 style="color: #2c3e50;">Şifre Yenileme Talebi</h2>
            </td>
        </tr>

        <tr>
            <td style="padding-top: 10px; color: #333;">
                Merhaba <strong>${user.firstName!'Değerli Kullanıcı'}</strong>,
            </td>
        </tr>

        <tr>
            <td style="padding-top: 15px; color: #333;">
                ${realmName} hesabınız için bir şifre yenileme talebi alındı.
            </td>
        </tr>

        <tr>
            <td style="padding-top: 15px; color: #333;">
                Şifrenizi yenilemek için aşağıdaki butona tıklayabilirsiniz:
            </td>
        </tr>

        <tr>
            <td style="text-align: center; padding-top: 25px; padding-bottom: 25px;">
                <a href="${link}"
                   style="
                       background-color: #1976d2;
                       color: white;
                       padding: 12px 24px;
                       text-decoration: none;
                       border-radius: 5px;
                       display: inline-block;
                       font-weight: bold;">
                    Şifremi Yenile
                </a>
            </td>
        </tr>

        <tr>
            <td style="color: #555;">
                Bu bağlantı <strong>${linkExpiration}</strong> süre boyunca geçerlidir.
            </td>
        </tr>

        <tr>
            <td style="padding-top: 15px; color: #555;">
                Eğer bu talebi siz oluşturmadıysanız, bu emaili güvenle görmezden gelebilirsiniz.
            </td>
        </tr>

        <tr>
            <td style="padding-top: 30px; color: #888; font-size: 12px; border-top: 1px solid #eee;">
                Bu email otomatik olarak gönderilmiştir. Lütfen yanıtlamayınız.<br>
                © ${.now?string("yyyy")} ${realmName}
            </td>
        </tr>

    </table>

</body>
</html>