namespace IdMasker.Service
{
    public class ConfigurableMasker : Masker
    {
        public ConfigurableMasker(IConfiguration configuration) : base(
            configuration["Masker:Alphabet"]!,
            configuration["Masker:Salt"]!
        ) { }
    }
}
