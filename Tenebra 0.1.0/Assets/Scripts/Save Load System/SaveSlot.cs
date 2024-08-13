using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSlot : MonoBehaviour
{
    public int slotNumber; // Bu save slotun numaras�
    private SaveLoadSystem saveLoadSystem;
    private GameController gameController;

    private void Start()
    {
        saveLoadSystem = FindObjectOfType<SaveLoadSystem>();
        gameController = FindObjectOfType<GameController>();
    }

    public void OnSlotClicked()
    {
        // Save veya load i�lemi i�in slot numaras�n� di�er scriptlere aktar
        saveLoadSystem.currentSlot = slotNumber;
        gameController.currentSlot = slotNumber;

        // Slot t�kland���nda kaydetme veya y�kleme i�lemini ba�latabilirsiniz
        if (MainMenu.instance.isLoadGame)
        {
            gameController.LoadGame();
        }
        else if(MainMenu.instance.isNewGame)
        {
            gameController.currentSlot = slotNumber;
            gameController.SaveGame();
            SceneManager.LoadScene("Hub");
        }
    }
}
