<#import "template.ftl" as layout>
<@layout.registrationLayout displayMessage=true; section>

<#if section == "form">
<form id="kc-form-login" action="${url.loginAction}" method="post">

    <div class="form-group">
        <label for="username">Kullanıcı Adı / Email</label>
        <input id="username"
               name="username"
               type="text"
               value="${(login.username!'')}"
               autofocus
               autocomplete="username"
               placeholder="kullanici@ornek.com"
               required />
    </div>

    <div class="form-group">
        <label for="password">Şifre</label>
        <input id="password"
               name="password"
               type="password"
               autocomplete="current-password"
               placeholder="••••••••"
               required />
    </div>

    <div class="form-actions">
        <button type="submit">Giriş Yap</button>
        <a href="${url.loginRestartFlowUrl}" class="cancel">İptal</a>
    </div>

</form>
</#if>

</@layout.registrationLayout>
