using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public static class BadWordFilter {

    static string[] patterns = { 
        "\\b[a@]ss\\b",
        "fuck",
        "bitch",
        "\\bكس\\b"
    };

    public static string Filter(string str)
    {
        return Regex.Replace(str, string.Join("|", patterns), a => new string('*', a.Length));
    }
}
