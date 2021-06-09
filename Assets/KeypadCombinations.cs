/*MESSAGE TO ANY FUTURE CODERS:
 PLEASE COMMENT YOUR WORK
 I can't stress how important this is especially with bomb types such as boss modules.
 If you don't it makes it realy hard for somone like me to find out how a module is working so I can learn how to make my own.
 Please comment your work.
 Short_c1rcuit*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using KModkit;

public class KeypadCombinations : MonoBehaviour {

	public KMBombInfo bomb;
	public KMAudio Audio;

    //Used for the ruleseed mod
    public KMRuleSeedable ruleSeedable;

    //Table that contains all the possible passwords
    int[] table = new int[100];

	//Tables to hold the numbers each button can display
	int[,] buttonnum = new int[4, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };

	//Array that holds the number on each button
	public TextMesh[] numbertext;

	//array for the buttons to be accessed
	public KMSelectable[] buttons;
	
	//The selectable display
	public KMSelectable display;

	//The answer for the module
	string answer;
	
	//Text on the display
	public TextMesh displaytext;

	//Colour of the text
	public Color[] fontcolours;

	//logging
	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	//Twitch help message
#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Use !{0} cycle to cycle through the options for each button. Submit the answer with “!{0} (code)”. For example: “!{0} 2806” to input 2806.";
#pragma warning restore 414

	public IEnumerator ProcessTwitchCommand(string command)
	{
		command = command.ToLowerInvariant().Trim();

		//When the user wants to cycle, press each button 3 times with a gap between each button press 
		if (command.Equals("cycle"))
		{
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					yield return null;
					buttons[i].OnInteract();
					yield return new WaitForSecondsRealtime(0.9f);
				}
			}
		}
		//If the input is 4 characters, cycle through each button till you get the desired one.
		else if (Regex.IsMatch(command, @"^\d{4}"))
		{
			command = command.Substring(0).Trim();

			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					if (numbertext[i].text != command[i].ToString())
					{
						yield return null;
						buttons[i].OnInteract();
						yield return new WaitForSecondsRealtime(0.1f);
					}
				}
				//However, if you cycle through all the different posibilities without the desired one appearing then send an error message 
				if (numbertext[i].text != command[i].ToString())
				{
					yield return "sendtochaterror One of the inputted numbers is not available.";
					yield break;
				}
			}
			//One you have set all the buttons to the desired output then press the display to submit
			yield return null;
			display.OnInteract();
		}
		else
		{
			//If the command sent isn't valid send an error
			yield return "sendtochaterror The inputted command is not valid.";
		}
	}

	//The force solve works by running a twitch command to submit an answer with the answer being the correct one
	public IEnumerator TwitchHandleForcedSolve()
	{
		IEnumerator enumerator = ProcessTwitchCommand(answer);
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
	}


	void Awake()
	{
		moduleId = moduleIdCounter++;
		foreach (KMSelectable button in buttons)
		{
			KMSelectable pressedButton = button;
			button.OnInteract += delegate () { ButtonPress(pressedButton); return false; };
		}

		display.OnInteract += delegate () { DisplayPress(); return false; };
	}

	// Use this for initialization
	void Start ()
	{
        var rnd = ruleSeedable.GetRNG();
        //Uses the origanal set of numbers if the ruleseed is one
        if (rnd.Seed == 1)
        {
            table = new int[100] { 3803, 2702, 9080, 7832, 1786, 8993, 0174, 6911, 2754, 0837, 7965, 7942, 0849, 7047, 7265, 2534, 3873, 0719, 6564, 7976, 1084, 7164, 9075, 2840, 2071, 0787, 1878, 4325, 2806, 1548, 5127, 5295, 9039, 4816, 3441, 0821, 6966, 7284, 4719, 7067, 4387, 2984, 1723, 6337, 7094, 4873, 1460, 1953, 4787, 1934, 6371, 9372, 1544, 9041, 1809, 1762, 9359, 2948, 5325, 5336, 6231, 8893, 1211, 4943, 3545, 7958, 4144, 8854, 4763, 4469, 9600, 3875, 6298, 4783, 9878, 7892, 1978, 2795, 4896, 5732, 1870, 6874, 5176, 9685, 8978, 8989, 4522, 8176, 6821, 1911, 0908, 0718, 1677, 8653, 0982, 8742, 8974, 7778, 8198, 9972 };
        }
        else
        {
            List<int> numbers = Enumerable.Range(0, 10000).ToList();
            rnd.ShuffleFisherYates(numbers);
			table = numbers.Take(100).ToArray();
        }

        GenerateCombination();

		Debug.LogFormat("[Keypad Combinations #{0}] The needed number is {1}", moduleId, answer);

		//Sets the text of each button to one of their possibilities selected at random
		for (int i = 0; i < 4; i++)
		{
			numbertext[i].text = buttonnum[i, UnityEngine.Random.Range(0, 3)].ToString();
		}
		
	}

	void GenerateCombination()
	{
		//Grabs a random number from the table and adds it to the list of the answers
		answer = table[UnityEngine.Random.Range(0, 100)].ToString();

		//Because of the way the numbers are stored (and that I'm too lazy to add in 100 sets of qutation marks)
		//certain numbers that zeroes at the front get shortened which can break the code
		//to prevent this I add an if statement that adds the zeroes back onto the the string form of the number
		if (answer.Length < 4)
		{
			answer = "0" + answer;
		}

		//Adds the digits to part of the buttontext arrays
		for (int i = 0; i < 4; i++)
		{
			//Minuses 48 eight to get the actual number
			buttonnum[i, 0] = answer[i] - 48;
		}

		//Adds two other random numbers to the button text arrays and makes sure that they are all different
		for (int i = 0; i < 4; i++)
		{
			List<int> inarray = new List<int> { buttonnum[i, 0] };
			for (int j = 1; j < 3; j++)
			{
				buttonnum[i, j] = UnityEngine.Random.Range(0, 10);
				if (inarray.Contains(buttonnum[i, j]))
				{
					j -= 1;
				}
				else
				{
					inarray.Add(buttonnum[i, j]);
				}
			}
		}

		//I use nested loop to check through all the button combinations for any other answers efficiently
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					for (int l = 0; l < 3; l++)
					{
						int testnum = (buttonnum[0, i] * 1000) + (buttonnum[1, j] * 100) + (buttonnum[2, k] * 10) + buttonnum[3, l];
						string testnumstring = testnum.ToString();

						//Same as earlier, the script adds a 0 to the front of the number to make it 4 characters long
						if (testnumstring.Length < 4)
						{
							testnumstring = "0" + testnumstring;
						}

						if (table.Contains(testnum) & testnumstring != answer)
						{
							//If another number reapears then it will retry with a different set of numbers.
							GenerateCombination();
						}
					}
				}
			}
		}
	}

	void ButtonPress(KMSelectable button)
	{
		if (moduleSolved)
		{
			return;
		}
		button.AddInteractionPunch();
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		int[] neededbuttonnum = new int[3];
		//This line cycles the button number to the next one on its section of the array
		System.Buffer.BlockCopy(buttonnum, Array.IndexOf(buttons, button) * 12, neededbuttonnum, 0, 12);
		numbertext[Array.IndexOf(buttons,button)].text = neededbuttonnum[(Array.IndexOf(neededbuttonnum, int.Parse(numbertext[Array.IndexOf(buttons, button)].text)) + 1) % 3].ToString();
	}

	void DisplayPress()
	{
		if (moduleSolved)
		{
			return;
		}
		
		display.AddInteractionPunch();
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		//Generates the answer to check in the if statement
		string answernum = numbertext[0].text + numbertext[1].text + numbertext[2].text + numbertext[3].text;
		Debug.LogFormat("[Keypad Combinations #{0}] You submitted {1}", moduleId, answernum);
		
		//If that was the correct combination of buttons
		if (answer == answernum)
		{
			//Solves the module
			moduleSolved = true;
			GetComponent<KMBombModule>().HandlePass();
			Debug.LogFormat("[Keypad Combinations #{0}] Module solved", moduleId);
			StartCoroutine(Solved());
		}
		else
		{
			//Resets the module
			GetComponent<KMBombModule>().HandleStrike();
			Debug.LogFormat("[Keypad Combinations #{0}] Incorrect", moduleId);
			StartCoroutine(Strike());
		}
		
	}

	IEnumerator Solved()
	{
		//Cycles the word "CONGRATS!" on the screen by taking a 4 character long substring and moving the starting point every 0.3 seconds.
		string solvetext = "CONGRATS!";
		displaytext.color = fontcolours[1];
		for (int i = 0; i < 6; i++)
		{
			displaytext.text = solvetext.Substring(i, 4);
			yield return new WaitForSeconds(0.3f);
		}
		displaytext.text = "____";
		displaytext.color = fontcolours[0];
	}

	IEnumerator Strike()
	{
		//Flashes "NOPE" on the screen in red for two seconds then returns to the regular text
		displaytext.color = fontcolours[2];
		displaytext.text = "NOPE";
		yield return new WaitForSeconds(2.0f);
		displaytext.color = fontcolours[0];
		displaytext.text = "____";
	}
}
