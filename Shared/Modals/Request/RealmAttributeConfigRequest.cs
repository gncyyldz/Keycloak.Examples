namespace Shared.Modals.Request
{
    public record RealmAttributeConfigRequest(List<AttributeDefinition> NewAttributes);

    public record AttributeDefinition(
         string Name,                          // Attribute adı (örn: "sicilNo")
         string? DisplayName,                  // Gösterim adı (örn: "Sicil Numarası")
         bool Multivalued,                     // Çoklu değer alabilir mi?
         bool Required,                        // Zorunlu mu?
         List<ValidationRule>? Validations     // Validation kuralları
                            );
    public record ValidationRule(
        string Type,              // Validation tipi: "length", "email", "pattern", "options", "uri", "integer", "number"
        int? Min,                 // Minimum değer (length, integer, number için)
        int? Max,                 // Maximum değer (length, integer, number için)
        string? Pattern,          // Regex pattern (pattern validation için)
        string? ErrorMessage,     // Hata mesajı
        List<string>? Options     // Seçenekler (options validation için)
                            );
}
