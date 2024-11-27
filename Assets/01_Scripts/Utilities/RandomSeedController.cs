using UnityEngine;
using System.Security.Cryptography;
using System.Text;

public class RandomSeedController : MonoBehaviour
{
    [SerializeField] private string seed;

    private void Awake() 
    {
        if (string.IsNullOrWhiteSpace(seed)){GenerateRandomSeed();}
    }

    public int GenerateRandomSeed()
    {
        int tempSeed = Random.Range(int.MinValue, int.MaxValue);
        seed = tempSeed.ToString();
        return tempSeed;
    }

    public string GetCurrentSeed(){return seed;}

    public int GetCurrentSeedAsInt()
    {
        return IsNumeric(seed) ? int.Parse(seed) : GetDeterministicHashCode(seed);
    }

    private bool IsNumeric(string s) => int.TryParse(s, out _);

    private int GetDeterministicHashCode(string seed)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(seed));
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
