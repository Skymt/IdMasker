namespace IdMasker.Service;

public class ConfigurableMasker(IConfiguration configuration) : Masker(
    configuration["IdMasker:Alphabet"]!,
    configuration["IdMasker:Salt"]!
)
{ }
