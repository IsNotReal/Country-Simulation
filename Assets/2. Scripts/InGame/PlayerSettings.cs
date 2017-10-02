using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettings : MonoBehaviour {
	[Header("Game Data")]
	public static int PlayerNumber = 1;
	public static string PlayerName = "PlayerName";
	public static string TeamName = "TeamName";
	public static int TeamNumber = 2;
	public static int GameLength = 2;
	public static int GameDifficulty = 2;

	[Header("System Data")]
	public static float MusicSound = 1f;
	public static float AudioSound = 1f;
}
