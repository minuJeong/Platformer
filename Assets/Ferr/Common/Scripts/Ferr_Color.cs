using UnityEngine;
using System.Collections;
using System;

public static class Ferr_Color {
	public static Color FromHex(string aHex) {
		if (aHex.Length != 8) return Color.red;
		return new Color( 
			Convert.ToInt32(""+aHex[0]+aHex[1])/255f,
			Convert.ToInt32(""+aHex[2]+aHex[3])/255f,
			Convert.ToInt32(""+aHex[4]+aHex[5])/255f,
			Convert.ToInt32(""+aHex[6]+aHex[7])/255f );
	}
	public static string ToHex(Color aColor) {
		return string.Format("{0:X}{1:X}{2:X}{3:X}", 
			(int)(aColor.r * 255),
			(int)(aColor.g * 255),
			(int)(aColor.b * 255),
			(int)(aColor.a * 255));
	}
}
