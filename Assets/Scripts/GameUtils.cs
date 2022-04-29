using UnityEngine;

// Game utils
public static class GameUtils
{
	public static void Enable(this CanvasGroup cg)
	{
		cg.blocksRaycasts = true;
		cg.interactable = true;
		cg.alpha = 1;
	}

	public static void Disable(this CanvasGroup cg)
	{
		cg.blocksRaycasts = false;
		cg.interactable = false;
		cg.alpha = 0;
	}
}
