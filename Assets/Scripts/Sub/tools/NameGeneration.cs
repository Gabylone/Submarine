using UnityEngine;
using System.Collections;

public class NameGeneration : MonoBehaviour {

	public static NameGeneration Instance;

	void Awake () {
		Instance = this;
	}

	private char[] vowels = new char[6] {
		'a','e','y','u','i','o'
	};

	private char[] consumn = new char[20] {
		'z', 'r', 't', 'p', 'q', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'w', 'x', 'c', 'v', 'b', 'n'
	};

	string syllab {
		get {
			return consumn[Random.Range(0,consumn.Length)].ToString () + vowels[Random.Range(0,vowels.Length)].ToString ();
		}
	}

	public string randomWord {
		get {

			string word = "";
			for (int i = 0; i < Random.Range (2, 4); ++i)
				word += syllab;

			return word.Remove (1).ToUpper () + word.Remove (0,1);
		}
	}
}
