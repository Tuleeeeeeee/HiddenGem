using Tuleeeeee.GameSystem;
using UnityEngine;

public class UIHanler : MonoBehaviour
{
   public void AddPickaxe()
   {
      var levelSystem = LevelSystem.Instance;
      levelSystem.AddPickaxe();
   }
}
