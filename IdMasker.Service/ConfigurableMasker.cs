namespace IdMasker.Service
{
    public class ConfigurableMasker(IConfiguration configuration) : Masker(
        configuration["Masker:Alphabet"]!,
        configuration["Masker:Salt"]!
        ) { }
}
