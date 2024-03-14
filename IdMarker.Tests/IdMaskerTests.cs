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
        string knownMask = "GtUVvwqvHmyQUIH5KD60";

        var mask = masker.Mask(ids);
        Assert.AreEqual(knownMask, mask);
        CollectionAssert.AreEqual(ids, masker.Unmask(mask).ToList());
    }

    [TestMethod]
    public void T3_Edges()
    {
        Masker masker = new();

        var mask = masker.Mask([ulong.MinValue]);
        Assert.AreEqual(ulong.MinValue, masker.Unmask(mask).Single());

        mask = masker.Mask([ulong.MaxValue]);
        Assert.AreEqual(ulong.MaxValue, masker.Unmask(mask).Single());
    }

    [TestMethod]
    public void T4_Padding()
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
    public void T5_Salt()
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
    public void T6_Alphabet()
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

    [TestMethod]
    public void T7_TestDirtyWords()
    {
        Masker masker = new();
        var id = 7501417ul;
        var dirtyWord = "Fi0D"; // Shame on you if you say this out loud! :P

        var mask1 = masker.Mask([id], minLength: 12);
        Assert.IsTrue(mask1.Contains(dirtyWord));
        Assert.AreEqual(id, masker.Unmask(mask1).Single());

        if(mask1.Contains(dirtyWord))
        {
            // Dirty word detected - regenerate the mask with an increment.
            var mask2 = masker.Mask([7501417], minLength: 12, increment: 1);
            Assert.IsFalse(mask2.Contains(dirtyWord));
            Assert.AreEqual(id, masker.Unmask(mask2).Single());
        }
    }
}