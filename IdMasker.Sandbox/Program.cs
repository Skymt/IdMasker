using IdMasker;

Masker masker = new();
Console.WriteLine("Welcome to the IdMasker demo!");
Console.WriteLine("This is a simple tool to mask and unmask numerical IDs.");

var sampleId = (ulong)Random.Shared.Next(10000000, 100000000); var sampleMask = masker.Mask([sampleId]);
Console.WriteLine($"E.g masking {sampleId} yields the string '{sampleMask}'.");
Console.WriteLine($"Unmasking converts it back into {masker.Unmask(sampleMask).Single()}.\n");

Console.WriteLine("The masker work by having an allowed set of symbols, an alphabet, that it shuffles around.");
Console.WriteLine("The id is then encoded by changing the digits and base to alphabet symbols and length.\n");

Console.WriteLine("The first character of the mask is a 'shuffle seed', used for the first shuffle when masking/unmasking.");
Console.WriteLine("This character is selected based on the ID itself, which ensures variation in the generated masks.");
Console.WriteLine($"5120001 => '{masker.Mask([5120001])}'");
Console.WriteLine($"5120002 => '{masker.Mask([5120002])}'\n");

Console.WriteLine("Characters are also shuffled based on the 'salt' of the masker. Same alphabet, but another salt yields");
Console.WriteLine($"5120001 => '{new Masker(salt: "First salt").Mask([5120001])}' (First salt)");
Console.WriteLine($"5120001 => '{new Masker(salt: "Second salt").Mask([5120001])}' (Second salt)\n");

Console.WriteLine("The mask may then be terminated by eos, a guard character and padding, or a separator character to split");
Console.WriteLine("this mask from other masks. Both the guard and the separator are part of the shuffled alphabet.\n");

Console.WriteLine("Padding is used to ensure the mask has a certain length. The default is 0, but can be set to any length.");
Console.WriteLine($"Without padding, the id 12 masks to '{masker.Mask([12])}', but this makes it easier for snoopers to guess the range.");
Console.WriteLine($"Padding to 7 characters: '{masker.Mask([12], 7)}' => 12");
Console.WriteLine($"Comparison with max int: '{masker.Mask([int.MaxValue], 7)}' => {int.MaxValue}\n");

Console.WriteLine("By default, all lower and uppcase letters as well as digits are used. More characters means shorter masks.");
Console.WriteLine($"uint.MaxValue: {masker.Mask([uint.MaxValue])} (7 characters)");
Console.WriteLine($"ulong.MaxValue: {masker.Mask([ulong.MaxValue])} (12 characters)\n");

Console.WriteLine("With a shorter alphabet, the mask has to grow in size to compensate.");
Console.WriteLine("The shortest possible alphabet is 4 characters, as two characters are reserved for guard and separator.");
Console.WriteLine("This would mean two symbols are left for the id, which is basically a binary representation.");
Console.WriteLine("To illustrate, the alphabet '01ab' is used here.");
Console.WriteLine($"uint.MaxValue - 2: {new Masker(alphabet: "01ab").Mask([uint.MaxValue - 2])} (33 characters)");
Console.WriteLine("(The first '1' is the shuffle seed then 32 symbols to represent the bits).\n");

// Create both random and sequential ids.
var aBunchOfIds = Enumerable.Range(0, 10).Select(_ => (ulong)Random.Shared.Next(1000000)).Union(Enumerable.Range(101000, 10).Select(v => (ulong)v)).ToArray();
var largeMask = masker.Mask(aBunchOfIds);
Console.WriteLine($"The separator character allows us to mask several ids at once! This is {aBunchOfIds.Length} ids masked as one string:");
Console.WriteLine(largeMask + '\n');

// Unmask the large mask and associate with the source ids by index - masking preserves order.
var sourceIdsAndUnmaskedIds = masker.Unmask(largeMask).Zip(aBunchOfIds);

Console.WriteLine("Source     Unmasked   Remasked   Unmasked");
foreach (var (unmasked_id, source_id) in sourceIdsAndUnmaskedIds)
{
    var singleMask = masker.Mask([source_id]);
    var singleUnmasked = masker.Unmask(singleMask).Single();

    Console.WriteLine($"{source_id:000000}     {unmasked_id:000000}     {singleMask}\t {singleUnmasked}");
}
