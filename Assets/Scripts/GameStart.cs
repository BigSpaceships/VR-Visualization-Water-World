using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        Invoke("ShowWelcomeText", 3f); // 3Ãëºóµ÷ÓÃ `ShowWelcomeText`
    }

    void ShowWelcomeText() {
        HUD_TextMessage hud = Object.FindFirstObjectByType<HUD_TextMessage>();
        if (hud != null) {
            hud.ShowText("Warning: Your oxygen will run out in 20 seconds.\nThis information will be sent to the Tourism Center.", null);
        }
    }
    // Update is called once per frame
    void Update() {

    }
}
