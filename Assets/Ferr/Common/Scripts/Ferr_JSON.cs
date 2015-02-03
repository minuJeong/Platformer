// COPYRIGHT: Do what you want, w/e. We're not responsible.
// VERSION  : 0.1 Initial
//
// AUTHOR : Nick Klingensmith, Simbryo
// TWITTER: @koujaku, @simbryo
// EMAIL  : support@simbryocorp.com
// WEBSITE: http://simbryocorp.com
//
// SUMMARY:
// A lightweight single file JSON parser, inplemented with Unity in mind. Focus is on ease of use, not performance. So far,
// it's taken everything I've thrown at it, but I would be more than interested to know if you spot a bug with it!
//
// EXAMPLE:
// Ferr_JSONValue player = Ferr_JSON.Parse("{name:\"koujaku\", speed:2, inventory:[1,2,3], stats:{health:1.0,XP:1337}}");
//
// Debug.Log( player["stats.health"] );
// Debug.Log( player["stats"]["XP" ] );
// player[ "speed"       ] = 4;
// player[ "stats.armor" ] = 1.0;
//
// Debug.Log(player);
// 
// OUTPUT:
// 1
// 1337
// {"name":"koujaku","speed":4,"inventory":[1,2,3],"stats":{"health":1,"XP":1337,"armor":1}}

using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

/// <summary>
/// JSON data types
/// String: 'string'
/// Number: 'float'
/// Object: 'Ferr_JSONObject[]'
/// Array : 'object[]'
/// Bool  : 'bool'
/// </summary>
public enum Ferr_JSONType {
	
	/// <summary>
	/// represented with a base 'string' type 
	/// </summary>
	String,
	/// <summary>
	/// represented with a base 'float' type
	/// </summary>
	Number,
	/// <summary>
	/// represented with a 'Ferr_JSONObject[]' type
	/// </summary>
	Object,
	/// <summary>
	/// represented with an 'object[]' type
	/// </summary>
	Array,
	/// <summary>
	/// represented with a 'bool' type 
	/// </summary>
	Bool
	
}

/// <summary>
/// Primary JSON data class. Provides implicit cast operators for float, double, bool, string, and object[]
/// variable types. Also provides [] access to contents via Set. 
/// EX:
/// jsonData["scene.player.name"] = "koujaku";
/// RESULT:
/// {"scene":{"player":{"name":"koujaku"}}}
/// </summary>
public class Ferr_JSONValue {
	public Ferr_JSONType type;
	public object        data;
	
	public int Length {
		get {
			if (type == Ferr_JSONType.Array ) return ((object[])data).Length;
			else if (type == Ferr_JSONType.Object) return ((Ferr_JSONObject[])data).Length;
			return 0;
		}
	}
	
	/// <summary>
	/// Creates a JSONValue with type Object, and data of null.
	/// </summary>
	public Ferr_JSONValue() {
		type = Ferr_JSONType.Object;
		data = null;
	}
	
	/// <summary>
	/// Automatically determines appropriate type for JSON object
	/// float, double, int and long get cast into a float container
	/// unknown types will throw an Exception
	/// </summary>
	public Ferr_JSONValue(object aData) {
		if (aData == null) {
			type = Ferr_JSONType.Object;
			data = null;
		} else if (aData is string) {
			type = Ferr_JSONType.String;
		} else if (aData is object[]) {
			type = Ferr_JSONType.Array;
	    } else if (aData is bool) {
			type = Ferr_JSONType.Bool;
	    } else if (aData is float || aData is double || aData is int || aData is long) {
			type  = Ferr_JSONType.Number;
			aData = (Convert.ToSingle(aData));
	    } else if (aData is Ferr_JSONObject) {
			type  = Ferr_JSONType.Object;
			aData = (object)(new Ferr_JSONObject[] { (Ferr_JSONObject)aData });
		} else {
			throw new Exception("Can't convert type '" + aData.GetType().Name + "' to a JSON value!");
		}
		data = aData;
	}
	
	/// <summary>
	/// Recursively converts a JSON string into a JSON data structure. Throws an exception
	/// if given an empty string. Unrecognized data is converted into a string.
	/// </summary>
	public void FromString(string aValue) {
		string text = aValue.Trim();
		float  fVal = 0;
		if (text.Length <= 0) throw new Exception("Bad JSON value: " + aValue);
		
		if (text[0] == '{') {
			type = Ferr_JSONType.Object;
			text = text.Remove(0,1);
			text = text.Remove(text.Length-1,1);
			string         [] items  = Ferr_JSON._Split(text, ',', true);
			Ferr_JSONObject[] result = new Ferr_JSONObject[items.Length];
			for (int i = 0; i < items.Length; i++) {
				result[i] = new Ferr_JSONObject(items[i]);
			}
			data = result;
		} else if (text[0] == '[') {
			type = Ferr_JSONType.Array;
			text = text.Remove(0,1);
			text = text.Remove(text.Length-1,1);
			string        [] items  = Ferr_JSON._Split(text, ',', true);
			Ferr_JSONValue[] result = new Ferr_JSONValue[items.Length];
			for (int i = 0; i < items.Length; i++) {
				result[i] = new Ferr_JSONValue();
				result[i].FromString (items[i]);
			}
			data = result;
		} else if (text[0] == '\"') {
			type = Ferr_JSONType.String;
			text = text.Remove(0,1);
			text = text.Remove(text.Length-1,1);
			data = text;
		} else if (float.TryParse(text, out fVal) ) {
			type = Ferr_JSONType.Number;
			data = fVal;
		} else {
			string lower = text.ToLower();
			if (lower == "false" || lower == "true") {
				type = Ferr_JSONType.Bool;
				data = bool.Parse(text);
			} else if (lower == "null") {
				type = Ferr_JSONType.Object;
				data = null;
			} else {
				type = Ferr_JSONType.String;
				data = text;
			}
		}
	}
	
	/// <summary>
	/// Gets the value at the specific path. Paths are separated using '.'s Ex:
	/// "scene.player.name", returns null if anything along the path cannot be found,
	/// case sensitive.
	/// </summary>
	public  Ferr_JSONValue  Get              (string aPath) {
		string[] words = aPath.Split ('.');
		Ferr_JSONValue curr = this;
		for (int i = 0; i < words.Length; i++) {
			Ferr_JSONObject child = curr.GetImmidiateChild(words[i]);
			if (child != null) {
				curr = child.val;
			} else {
				return null;
			}
		}
		return curr;
	}
	private Ferr_JSONObject GetImmidiateChild(string aName) {
		if (type == Ferr_JSONType.Object) {
			Ferr_JSONObject[] children = (Ferr_JSONObject[])data;
			if (children != null) {
				for (int i = 0; i < children.Length; i++) {
					if (children[i].name == aName)
						return children[i];
				}
			}
		}
		return null;
	}
	/// <summary>
	/// Creates or sets all values along the path to Object, last object on the path is
	/// given a null Object value.
	/// </summary>
	public  Ferr_JSONValue  Set              (string aPath) {
		return Set (aPath, new Ferr_JSONValue());
	}
	/// <summary>
	/// Creates or sets all values along the path to Object, last object on the path is
	/// set to the provided value.
	/// </summary>
	public  Ferr_JSONValue  Set              (string aPath, Ferr_JSONValue aVal) {
		string[]       words = aPath.Split('.');
		Ferr_JSONValue  curr    = this;
		Ferr_JSONObject currObj = null;
		for (int i = 0; i < words.Length; i++) {
			Ferr_JSONObject result = curr.GetImmidiateChild(words[i]);
			
			if (result == null) {
				Ferr_JSONObject[] list = null;
				if (curr.type == Ferr_JSONType.Object) {
					list = (Ferr_JSONObject[])curr.data;
					if (list == null) list = new Ferr_JSONObject[1];
					else Array.Resize<Ferr_JSONObject>(ref list, list.Length + 1);
					curr.data = list;
				} else {
					curr.type = Ferr_JSONType.Object;
					curr.data = list = new Ferr_JSONObject[1];
				}
				list[list.Length-1] = new Ferr_JSONObject(words[i], new Ferr_JSONValue());
				result = list[list.Length-1];
			}
			currObj = result;
			curr    = result.val;
		}
		
		if (currObj != null){
			currObj.val = aVal;
			return currObj.val;
		} else {
			return null;
		}
	}
	public  Ferr_JSONValue  Get              (int    aIndex) {
		if (type == Ferr_JSONType.Array) {
			object[] list = (object[])data;
			if (aIndex >=0 && aIndex < list.Length) {
				return (Ferr_JSONValue)((object[])data)[aIndex];
			}
			return null;
		} else {
			return null;
		}
	}
	public  Ferr_JSONValue  Set              (int    aIndex, Ferr_JSONValue aVal) {
		if (type == Ferr_JSONType.Array) {
			object[] list = (object[])data;
			if (list.Length <= aIndex) {
				Array.Resize<object>(ref list, aIndex+1);
				data = list;
			}
			list[aIndex] = aVal;
			return aVal;
		} else {
			type = Ferr_JSONType.Array;
			object[] list = new object[aIndex+1];
			list[aIndex] = aVal;
			data = list;
			return aVal;
		}
	}
	
	public override string ToString ()
	{
		if (type == Ferr_JSONType.Array) {
			string   result = "[";
			object[] array  = (object[])data;
			for (int i = 0; i < array.Length; i++) {
				string tmp = "";
				if (array[i] is Ferr_JSONValue) tmp = array[i].ToString ();
				else tmp = (new Ferr_JSONValue(array[i])).ToString ();
				result += tmp + ( i == array.Length-1 ? "" : "," );
			}
			result += "]";
			return result;
		} else if (type == Ferr_JSONType.Object) {
			string            result = "";
			Ferr_JSONObject[] array  = (Ferr_JSONObject[])data;
			if (array == null) {
				result = "null";
			} else {
				result += "{";
				for (int i = 0; i < array.Length; i++) {
					result += array[i].ToString () + ( i == array.Length-1 ? "" : "," );
				}
				result += "}";
			}
			return result;
		} else if (type == Ferr_JSONType.Bool) {
			return ((bool)data) ? "true" : "false";
		} else if (type == Ferr_JSONType.Number) {
			return ""+data;
		} else {
			return "\"" + data + "\"";
		}
	}
	
	public Ferr_JSONValue this[string aPath] {
		get { return Get(aPath); }
		set { Set(aPath, value); }
	}
	public Ferr_JSONValue this[int aIndex] {
		get { return Get (aIndex); }
		set { Set (aIndex, value); }
	}
	public bool     this[int aIndex, bool aDefaultValue] {
		get { Ferr_JSONValue val = Get (aIndex); return val == null || val.type != Ferr_JSONType.Bool    ? aDefaultValue: (bool     )val.data;}
	}
	public string   this[int aIndex, string aDefaultValue] {
		get { Ferr_JSONValue val = Get (aIndex); return val == null || val.type != Ferr_JSONType.String  ? aDefaultValue: (string   )val.data;}
	}
	public float    this[int aIndex, float aDefaultValue] {
		get { Ferr_JSONValue val = Get (aIndex); return val == null || val.type != Ferr_JSONType.Number  ? aDefaultValue: (float    )val.data;}
	}
	public object[] this[int aIndex, object[] aDefaultValue] {
		get { Ferr_JSONValue val = Get (aIndex); return val == null || val.type != Ferr_JSONType.Array   ? aDefaultValue: (object[] )val.data;}
	}
	
	public bool     this[string aPath, bool     aDefaultValue] { get { Ferr_JSONValue val = Get(aPath); return val == null || val.type != Ferr_JSONType.Bool   ? aDefaultValue: (bool    )val.data; } }
	public string   this[string aPath, string   aDefaultValue] { get { Ferr_JSONValue val = Get(aPath); return val == null || val.type != Ferr_JSONType.String ? aDefaultValue: (string  )val.data; } }
	public float    this[string aPath, float    aDefaultValue] { get { Ferr_JSONValue val = Get(aPath); return val == null || val.type != Ferr_JSONType.Number ? aDefaultValue: (float   )val.data; } }
	public object[] this[string aPath, object[] aDefaultValue] { get { Ferr_JSONValue val = Get(aPath); return val == null || val.type != Ferr_JSONType.Array  ? aDefaultValue: (object[])val.data; } }
	
	public static implicit operator Ferr_JSONValue(float    val) { return new Ferr_JSONValue(val); }
	public static implicit operator Ferr_JSONValue(double   val) { return new Ferr_JSONValue(val); }
	public static implicit operator Ferr_JSONValue(bool     val) { return new Ferr_JSONValue(val); }
	public static implicit operator Ferr_JSONValue(string   val) { return new Ferr_JSONValue(val); }
	public static implicit operator Ferr_JSONValue(object[] val) { return new Ferr_JSONValue(val); }
}

/// <summary>
/// A JSON 'object' in the form of "name": val
/// </summary>
public class Ferr_JSONObject {
	public string         name;
	public Ferr_JSONValue val;
	
	/// <summary>
	/// Creates a JSON object from given JSON text data.
	/// </summary>
	public Ferr_JSONObject(string aText) {
		FromString(aText);
	}
	/// <summary>
	/// Manually creates a JSON object 
	/// </summary>
	public Ferr_JSONObject(string aName, Ferr_JSONValue aVal) {
		name = aName;
		val  = aVal;
	}
	
	/// <summary>
	/// Overwrites data with information from the provided JSON. This method will throw an Exception
	/// if the text cannot be split into exactly two pieces (left and right of an initial ':')
	/// </summary>
	public void FromString(string aText) {
		aText = aText.Trim();
		string[] words = Ferr_JSON._Split(aText, ':', true);
		
		if (words.Length != 2) throw new Exception("Bad JSON Object: " + aText);
		name = words[0].Trim().Replace("\"", "");
		val  = new Ferr_JSONValue();
		val.FromString(words[1]);
	}
	public override string ToString ()
	{
		return "\"" + name + "\":" + (val==null?"null":val.ToString ());
	}
}

public static class Ferr_JSON {
	/// <summary>
	/// Any exceptions that may have been thrown during the Parse method, null if nothig bad happened.
	/// </summary>
	public static Exception error  = null;
	/// <summary>
	/// Did the last execution fail, or not?
	/// </summary>
	public static bool      failed = false;
	
	/// <summary>
	/// Parses the provided JSON into a JSON object. If errors occur, null will be returned, and error info 
	/// will be povided in the Ferr_JSON.error and Ferr_JSON.failed fields.
	/// </summary>
	/// <param name='aText'>
	/// JSON text data. Whitespace isn't important.
	/// </param>
	public static Ferr_JSONValue Parse      (string aText) {
		Ferr_JSONValue result = null;
		failed = false;
		error  = null;
		try {
			result = new Ferr_JSONValue();
			result.FromString(aText);
		} catch (Exception e) {
			error  = e;
			failed = true;
		}
		return result;
	}
	/// <summary>
	/// Creates a JSON object from the given object. Only uses public fields from that specific class, 
	/// not including parent classes. Also, only supports really basic variable types well.
	/// </summary>
	/// <returns>
	/// A JSON object that can be .ToString()ed into JSON data.
	/// </returns>
	/// <param name='obj'>
	/// A really simple object you want to convert into JSON.
	/// </param>
	public static Ferr_JSONValue MakeShallow(object obj) {
		Type           t      = obj.GetType();
		FieldInfo[]    fields = t  .GetFields(System.Reflection.BindingFlags.Public | BindingFlags.Instance);
		Ferr_JSONValue val    = new Ferr_JSONValue();
		
		for (int i = 0; i < fields.Length; i++) {
			object data = fields[i].GetValue(obj);
			if (data is float || data is int || data is double || data is long)
				val[fields[i].Name] = Convert.ToSingle(data);
			else if (data is string)
				val[fields[i].Name] = (string)data;
			else if (data is bool)
				val[fields[i].Name] = (bool)data;
			else
				val[fields[i].Name] = data.ToString ();
		}
		return val;
	}
	
	/// <summary>
	/// For internal use, splitting strings with attention paid to special characters.
	/// </summary>
	public static string[] _Split(string aText, char aDelimeter, bool aOuterOnly = false) {
		List<string> words    = new List<string>();
		string       currWord = "";
		bool         inString = false;
		int          inner    = 0;
		for (int i = 0; i < aText.Length; i++) {
			if (aText[i] == '\"'                  ) inString = !inString;
			if (aText[i] == '{' || aText[i] == '[') inner   += 1;
			if (aText[i] == '}' || aText[i] == ']') inner   -= 1;
			
			if (!(aOuterOnly && inner>0) && !inString && aText[i] == aDelimeter) {
				words.Add (currWord);
				currWord = "";
			} else {
				currWord += aText[i];
			}
		}
		if (currWord != "")
			words.Add(currWord);
		return words.ToArray ();
	}
}