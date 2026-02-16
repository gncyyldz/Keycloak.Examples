<#import "template.ftl" as layout>
<@layout.registrationLayout displayMessage=true; section>

<#if section == "form">
<form id="kc-form-otp" action="${url.loginAction}" method="post">

    <div class="form-group">
        <label for="otp">Doğrulama Kodu</label>
        <input id="otp"
               name="otp"
               type="text"
               autocomplete="one-time-code"
               autofocus
               maxlength="6"
               pattern="[0-9]*"
               inputmode="numeric"
               placeholder="6 haneli kod"
               required />
        <p class="input-help">Authenticator uygulamanızdan 6 haneli kodu girin</p>
    </div>

    <div class="form-actions">
        <button type="submit">Doğrula</button>
        <a href="${url.loginRestartFlowUrl}" class="cancel">İptal</a>
    </div>

</form>
</#if>

</@layout.registrationLayout>
