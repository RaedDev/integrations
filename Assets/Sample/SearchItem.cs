using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchItem : MonoBehaviour 
{
	public Text username;
	public Text bio;

	public void Setup(User user)
    {
		Button button = GetComponent<Button>();

		username.text = user.username;
		bio.text = user.bio;

		button.onClick.AddListener(() =>
		{
			FindObjectOfType<ProfilePanel>().Setup(user);
			SearchUsers.Close();
		});
    }
}
