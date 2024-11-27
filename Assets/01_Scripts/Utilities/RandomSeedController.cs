using UnityEngine;
using System.Security.Cryptography;
using System.Text;

public class RandomSeedController : MonoBehaviour
{
    [SerializeField] private string seed;

    private void Awake() 
    {
        // If the seed is null or whitespace, generate a new random seed
        if (string.IsNullOrWhiteSpace(seed)){GenerateRandomSeed();}
    }

    public int GenerateRandomSeed()
    {
        // Generate a random integer within the full range of int values
        int tempSeed = Random.Range(int.MinValue, int.MaxValue);
        seed = tempSeed.ToString();
        return tempSeed;
    }

    public string GetCurrentSeed(){return seed;}

    public int GetCurrentSeedAsInt()
    {
        // Convert the seed to an integer if it's numeric, otherwise compute a hash code
        return IsNumeric(seed) ? int.Parse(seed) : GetDeterministicHashCode(seed);
    }

    private bool IsNumeric(string s) => int.TryParse(s, out _);

    private int GetDeterministicHashCode(string seed)
    {
        // Compute a deterministic hash code using SHA256 for non-numeric seeds
        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(seed));
        // Convert the first 4 bytes of the hash to an integer
        return System.BitConverter.ToInt32(bytes, 0);
    }

    public void SetSeed()
    {
        Random.InitState(GetCurrentSeedAsInt());
    }

    public void SetSeed(int seed)
    {
        Random.InitState(seed);
    }
    
    public void CopySeedToClipboard() => GUIUtility.systemCopyBuffer = seed;

}
