using IdMasker;
using System.Numerics;

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

var clearTextPnr1 = "19800101T1234";
var clearTextPnr2 = "19770712-9876";
var maskedPnr1 = Encode(clearTextPnr1);
var maskedPnr2 = Encode(clearTextPnr2);
var unmaskedPnr1 = Decode(maskedPnr1);
var unmaskedPnr2 = Decode(maskedPnr2);
return 0;

string Encode(string pnr)
{
    var numerical_pnr = ConvertBase(pnr, "-T0987654321", "0123456789");
    return masker.Mask([ulong.Parse(numerical_pnr)]);
}
string Decode(string mask)
{
    var numerical_pnr = masker.Unmask(mask).Single().ToString();
    return ConvertBase(numerical_pnr, "0123456789", "-X0987654321");
}
string ConvertBase(string input, string sourceDigitSet, string targetDigitSet)
{
    BigInteger amount = 0;

    int positionExponent = 0;
    int sourceBase = sourceDigitSet.Length;
    int targetBase = targetDigitSet.Length;
    var inputStack = new Stack<char>(input);
    //For every digit, sum up its value, which is equal to its index in the digit set multiplied with the power of its position.
    while (inputStack.Count > 0) amount += sourceDigitSet.IndexOf(inputStack.Pop()) * BigInteger.Pow(sourceBase, positionExponent++);

    string result = string.Empty;
    //Add the digit from the target digit set that represents the remainder of the amount divided by the target base
    do result = targetDigitSet[(int)(amount % targetBase)] + result;
    //and store the amount of that division as integer while it doesn't equal 0.
    while ((amount /= targetBase) > 0);

    return result;
}