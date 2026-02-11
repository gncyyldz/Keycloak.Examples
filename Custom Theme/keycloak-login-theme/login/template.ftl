<#macro registrationLayout bodyClass="" displayInfo=false displayMessage=true displayRequiredFields=false showAnotherWayIfPresent=true>
<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="utf-8">
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
    <meta name="robots" content="noindex, nofollow">
    <meta name="viewport" content="width=device-width, initial-scale=1">

    <#if properties.meta?has_content>
        <#list properties.meta?split(' ') as meta>
            <meta name="${meta?split('==')[0]}" content="${meta?split('==')[1]}"/>
        </#list>
    </#if>

    <title>${msg("loginTitle",(realm.displayName!''))}</title>

    <link rel="icon" href="${url.resourcesPath}/img/favicon.ico" />
    
    <#if properties.styles?has_content>
        <#list properties.styles?split(' ') as style>
            <link href="${url.resourcesPath}/${style}" rel="stylesheet" />
        </#list>
    </#if>
    
    <link href="${url.resourcesPath}/css/login.css" rel="stylesheet" />
</head>

<body>
    <div class="stars"></div>
    <div class="login-container">
        <div class="login-box">
            <div class="login-header">
                <h1>Giriş Yap</h1>
                <#if client?? && client.clientId??>
                    <div class="client-info">
                        <div class="client-message">
                            <#if client.name?? && client.name?has_content>
                                <strong>${client.name}</strong> için yetki istenmektedir
                            <#else>
                                <strong>${client.clientId}</strong> için yetki istenmektedir
                            </#if>
                        </div>
                    </div>
                </#if>
                <#if realm.internationalizationEnabled && locale.supported?size gt 1>
                    <div class="language-selector">
                        <#list locale.supported as l>
                            <a href="${l.url}" class="${(locale.current == l.label)?then('active','')}">${l.label}</a>
                        </#list>
                    </div>
                </#if>
            </div>

            <div class="login-content">
                <#if displayMessage && message?has_content && (message.type != 'warning' || !isAppInitiatedAction??)>
                    <div class="alert alert-${message.type}">
                        <#if message.type = 'success'><span class="icon">✓</span></#if>
                        <#if message.type = 'warning'><span class="icon">⚠</span></#if>
                        <#if message.type = 'error'><span class="icon">✕</span></#if>
                        <#if message.type = 'info'><span class="icon">ⓘ</span></#if>
                        <span class="message-text">${kcSanitize(message.summary)?no_esc}</span>
                    </div>
                </#if>

                <#nested "form">

                <#if displayInfo>
                    <#nested "info">
                </#if>
            </div>

            <#if displayInfo>
                <div class="login-footer">
                    <#nested "info">
                </div>
            </#if>
        </div>
    </div>
</body>
</html>
</#macro>
