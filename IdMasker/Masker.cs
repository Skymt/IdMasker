﻿namespace IdMasker;
public class Masker
{
    readonly string _alphabet;
    readonly string _salt;
    public Masker(
        string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789",
        string salt = "The salt. Best to bring your own...")
    {
        if(alphabet.Length < 4) throw new ArgumentException("The alphabet must be at least 4 characters long.");

        _alphabet = Shuffle(alphabet, salt);
        _salt = salt;
    }


    public string Mask(IEnumerable<ulong> ids, int minLength = 0)
    {
        var firstId = ids.First();
        var seed = (ulong)(firstId + (ulong)ids.Count());
        var shuffleseed = _alphabet[(int)(seed % (ulong)_alphabet.Length)];
        var alphabet = Shuffle(_alphabet, shuffleseed + _salt);

        var mask = shuffleseed + Encode(firstId, alphabet[2..]);

        foreach (var id in ids.Skip(1))
        {
            mask += alphabet[1];
            alphabet = Shuffle(_alphabet, alphabet);
            mask += Encode(id, alphabet[2..]);
        }

        if (mask.Length < minLength)
        {
            mask += alphabet[0];
            alphabet = Shuffle(_alphabet, alphabet);
            while (mask.Length < minLength) 
                mask += alphabet[mask.Length % alphabet.Length];
        }
        return mask;
    }
    public IEnumerable<ulong> Unmask(string mask)
    {
        var subMask = string.Empty;
        var alphabet = Shuffle(_alphabet, mask[0] + _salt);

        foreach (var c in mask.Skip(1))
        {
            if (c == alphabet[0]) break;
            else if (c == alphabet[1])
            {
                yield return Decode(subMask, alphabet[2..]);
                alphabet = Shuffle(_alphabet, alphabet);
                subMask = string.Empty;
            }
            else subMask += c;
        }

        if (!string.IsNullOrEmpty(subMask))
            yield return Decode(subMask, alphabet[2..]);
    }

    static string Shuffle(string characters, string salt)
    {
        var chars = characters.ToCharArray();
        int salter = 0, accumulator = 0, salt_value;
        for (int target = chars.Length - 1; target > 0; target--, salter++)
        {
            accumulator += salt_value = salt[salter %= salt.Length];
            var source = (salt_value + salter + accumulator) % target;
            (chars[target], chars[source]) = (chars[source], chars[target]);
        }

        return new(chars);
    }
    static string Encode(ulong id, string alphabet)
    {
        ulong alphabetLength = (ulong)alphabet.Length, remainder;
        var mask = string.Empty;
        do
        {
            (id, remainder) = Math.DivRem(id, alphabetLength);
            mask += alphabet[(int)remainder];
        } while (id > 0);

        return mask;
    }
    static ulong Decode(string mask, string alphabet)
    {
        var id = 0ul;
        var inputChars = mask.ToCharArray();
        var alphabetLength = (ulong)alphabet.Length;
        for (var i = 0; i < mask.Length; i++)
        {
            var position = mask.Length - 1 - i;
            var value = (ulong)alphabet.IndexOf(inputChars[position]);
            var magnitude = 1ul;
            for (var j = 0; j < position; j++) magnitude *= alphabetLength;
            id += value * magnitude;
        }

        return id;
    }
}








// SaferMasker uses an end-of-mask character to terminate each mask
// which makes it harder to guess valid masks. If the marker is missing
// no ids will be decoded at all.
// A long random mask, may produce some values, but the chance is greater
// that the mask will overflow the ulong datatype instead.
// 
// An upside is that only one character of the alphabet is reserved for special use.
// But the mask will be one character longer than the other maskers masks.
public class SaferMasker
{
    readonly string _alphabet;
    readonly string _salt;
    public SaferMasker(
        string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789",
        string salt = "The salt. Best to bring your own...")
    {
        if (alphabet.Length < 3) throw new ArgumentException("The alphabet must be at least 3 characters long.");

        _alphabet = Shuffle(alphabet, salt);
        _salt = salt;
    }

    public string Mask(IEnumerable<ulong> ids, int minLength = 0)
    {
        var firstId = ids.First();
        var shuffleseed = _alphabet[(int)((firstId + (ulong)ids.Count()) % (ulong)_alphabet.Length)];
        var alphabet = Shuffle(_alphabet, shuffleseed + _salt);

        var mask = shuffleseed + Encode(firstId, alphabet[1..]) + alphabet[0];

        foreach (var id in ids.Skip(1))
        {
            alphabet = Shuffle(_alphabet, alphabet);
            mask += Encode(id, alphabet[1..]) + alphabet[0];
        }

        if (mask.Length < minLength)
        {
            alphabet = Shuffle(_alphabet, alphabet)[1..];
            var counter = mask.Length;
            while (mask.Length < minLength)
                mask += alphabet[counter++ % alphabet.Length];
        }
        return mask;
    }
    public IEnumerable<ulong> Unmask(string mask)
    {
        var subMask = string.Empty;
        var alphabet = Shuffle(_alphabet, mask[0] + _salt);

        foreach (var c in mask.Skip(1))
        {
            if (c == alphabet[0])
            {
                yield return Decode(subMask, alphabet[1..]);
                alphabet = Shuffle(_alphabet, alphabet);
                subMask = string.Empty;
            }
            else subMask += c;
        }
    }

    static string Shuffle(string characters, string salt)
    {
        var chars = characters.ToCharArray();
        int salter = 0, accumulator = 0, salt_value;
        for (int target = chars.Length - 1; target > 0; target--, salter++)
        {
            accumulator += salt_value = salt[salter %= salt.Length];
            var source = (salt_value + salter + accumulator) % target;
            (chars[target], chars[source]) = (chars[source], chars[target]);
        }

        return new(chars);
    }
    static string Encode(ulong id, string alphabet)
    {
        ulong alphabetLength = (ulong)alphabet.Length, remainder;
        var mask = string.Empty;
        do
        {
            (id, remainder) = Math.DivRem(id, alphabetLength);
            mask += alphabet[(int)remainder];
        } while (id > 0);

        return mask;
    }
    static ulong Decode(string mask, string alphabet)
    {
        var id = 0ul;
        var inputChars = mask.ToCharArray();
        var alphabetLength = (ulong)alphabet.Length;
        for (var i = 0; i < mask.Length; i++)
        {
            var position = mask.Length - 1 - i;
            var value = (ulong)alphabet.IndexOf(inputChars[position]);
            var magnitude = 1ul;
            for (var j = 0; j < position; j++) magnitude *= alphabetLength;
            id += value * magnitude;
        }

        return id;
    }
}