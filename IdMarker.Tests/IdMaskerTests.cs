namespace IdMasker.Tests;

[TestClass]
public class IdMaskerTests
{
    [TestMethod]
    public void T1_Single()
    {
        Masker masker = new();
        ulong id = 100001; string knownMask = "i8QN";

        var mask = masker.Mask([id]);
        var unmaskedId = masker.Unmask(mask).Single();
        
        Assert.AreEqual(knownMask, mask);
        Assert.AreEqual(id, unmaskedId);
    }

    [TestMethod]
    public void T2_Collection()
    {
        Masker masker = new();
        ulong[] ids = [100001, 100002, 100003, 100004, 100005];

        var mask = masker.Mask(ids);
        CollectionAssert.AreEqual(ids, masker.Unmask(mask).ToList());
    }

    [TestMethod]
    public void T3_Padding()
    {
        Masker masker = new();
        ulong id = 100001;
        
        var mask = masker.Mask([id]);
        Assert.IsTrue(mask.Length < 8);
        Assert.AreEqual(id, masker.Unmask(mask).Single());

        mask = masker.Mask([id], minLength: 8);
        Assert.AreEqual(mask.Length, 8);
        Assert.AreEqual(id, masker.Unmask(mask).Single());
    }

    [TestMethod]
    public void T4_Salt()
    {
        Masker masker1 = new(salt: "This is the first salt");
        Masker masker2 = new(salt: "The second salt is different");
        ulong id = 100001;

        var mask1 = masker1.Mask([id]);
        var mask2 = masker2.Mask([id]);
        Assert.AreNotEqual(mask1, mask2);
        Assert.AreEqual(id, masker1.Unmask(mask1).Single());
        Assert.AreEqual(id, masker2.Unmask(mask2).Single());
    }

    [TestMethod]
    public void T5_Alphabet()
    {
        Masker masker = new(alphabet: "abcdefABCDEF");
        ulong id = 100001;

        var mask = masker.Mask([id], 20);
        foreach (var c in mask)
        {
            Assert.IsTrue("abcdefABCDEF".Contains(c));
        }
        Assert.AreEqual(id, masker.Unmask(mask).Single());
    }
}