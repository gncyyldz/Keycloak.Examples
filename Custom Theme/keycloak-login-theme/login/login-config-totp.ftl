<#import "template.ftl" as layout>
<@layout.registrationLayout displayMessage=true; section>

<#if section == "form">
<div class="totp-container">
    <div class="totp-qr-section">
        <h3>1. Uygulamayı İndirin</h3>
        <p class="totp-instruction">Google Authenticator veya benzeri bir uygulama indirin.</p>
        
        <h3>2. QR Kodu Okutun</h3>
        <div class="qr-code-wrapper">
            <img src="data:image/png;base64, ${totp.totpSecretQrCode}" alt="QR Kod" class="qr-code">
        </div>
        
        <div class="manual-entry">
            <p class="totp-instruction">QR kodu okutamıyorsanız, manuel olarak girebilirsiniz:</p>
            <div class="secret-key">
                <code>${totp.totpSecretEncoded}</code>
            </div>
        </div>
    </div>
    
    <div class="totp-form-section">
        <h3>3. Kodu Doğrulayın</h3>
        <p class="totp-instruction">Uygulamada görünen 6 haneli kodu girin:</p>
        
        <form action="${url.loginAction}" method="post" id="kc-totp-settings-form">
            <input type="hidden" name="totpSecret" value="${totp.totpSecret}" />
            <#if mode??><input type="hidden" name="mode" value="${mode}"/></#if>
            
            <div class="form-group">
                <label for="totp">Doğrulama Kodu</label>
                <input id="totp" 
                       name="totp" 
                       type="text" 
                       autocomplete="off"
                       autofocus
                       maxlength="6"
                       pattern="[0-9]*"
                       inputmode="numeric"
                       placeholder="123456"
                       required />
            </div>
            
            <div class="form-group">
                <label for="userLabel">Cihaz Adı (Opsiyonel)</label>
                <input id="userLabel" 
                       name="userLabel" 
                       type="text" 
                       autocomplete="off"
                       placeholder="Örn: Telefonum"
                       value="${(totp.userLabel!'')}" />
            </div>

            <div class="form-actions">
                <button type="submit">Kaydet ve Devam Et</button>
                <a href="${url.loginRestartFlowUrl}" class="cancel">İptal</a>
            </div>
        </form>
    </div>
</div>
</#if>

</@layout.registrationLayout>
