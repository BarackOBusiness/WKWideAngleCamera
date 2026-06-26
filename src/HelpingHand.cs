using UnityEngine;

namespace WideAngleCamera;

public class HelpingHand : MonoBehaviour {
	private ENT_Player player;

	private void Update() {
		if (player != null) {
			foreach (var hand in player.hands) {
				if (hand.IsHolding() || hand.IsLocked()) {
					hand.handBase.gameObject.layer = 28;
				} else {
					hand.handBase.gameObject.layer = 8;
				}
			}
		} else {
			player = ENT_Player.GetPlayer();
		}
	}
}
