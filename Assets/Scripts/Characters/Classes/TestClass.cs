using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class TestClass : PlayableChar
{
    #region stats

    protected List<string> abilityList;
    protected List<string> spellList;

    /// <summary>
    /// Gets character's current health
    /// </summary>
    public override int Health
    {
        get { return health; }
    }
    /// <summary>
    /// Gets character's maximum health
    /// </summary>
    public override int MaxHealth
    {
        get { return maxHealth; }
    }
    /// <summary>
    /// Character's current MP
    /// </summary>
    public override int MutationPoints
    {
        get { return mutationPoints; }
    }
    /// <summary>
    /// Character's maximum MP
    /// </summary>
    public override int MaxMutationPoints
    {
        get { return MaxMutationPoints; }
    }
    /// <summary>
    /// Gets character's movement speed
    /// </summary>
    public override int Speed
    {
        get { return speed; }
    }
    /// <summary>
    /// Gets character's max speed
    /// </summary>
    public override int MaxSpeed
    {
        get { return maxSpeed; }
    }
    /// <summary>
    /// Gets character's attack. Used for physical damage
    /// </summary>
    public override int Attack
    {
        get { return attack; }
    }
    /// <summary>
    /// Gets character's magic attack. Used for magic damage
    /// </summary>
    public override int MagicAttack
    {
        get { return magicAttack; }
    }
    /// <summary>
    /// Gets character's defense. Used to defend against physical attacks
    /// </summary>
    public override int Defense
    {
        get { return defense; }
    }
    /// <summary>
    /// Gets character's resistance. Used to defend against magical attacks
    /// </summary>
    public override int Resistance
    {
        get { return resistance; }
    }
    /// <summary>
    /// Gets character's attack range
    /// </summary>
    public override int AttackRange
    {
        get { return rangedWeapon.Range; }
    }

    /// <summary>
    /// Gets character's abilities
    /// </summary>
    public List<string> AbilityList
    {
        get { return abilityList; }
    }
    /// <summary>
    /// Gets character's spells
    /// </summary>
    public List<string> SpellList
    {
        get { return spellList; }
    }
    #endregion

    //not finished
    #region Equipment
    private Pistol rangedWeapon;
    private Sword meleeWeapon;



    #endregion




    private void Awake()
    {
        abilityList = new List<string>();
        spellList = new List<string>();

        rangedWeapon = new Pistol(5, 5, 3);
        meleeWeapon = new Sword(5);
    }


    /// <summary>
    /// highlights all enemies targetable from this space
    /// </summary>
    protected override void DrawTargets()
    {
        //destroy no longer relevant UI
        foreach (GameObject oldIndicator in GameObject.FindGameObjectsWithTag("SelectionIcon")) //destroys old targettable ui
        {
            Destroy(oldIndicator);
        }

        //dont draw targets if out of ammo
        if (rangedWeapon.Ammo <= 0) { return; }

        //dynamically shows which enemies can be attacked from the character's current position
        //gets list of enemies in range
        List<GameObject> enemyList = (from GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy") where System.Math.Abs((int)transform.position.x - enemy.transform.position.x) + System.Math.Abs((int)transform.position.y - enemy.transform.position.y) <= AttackRange select enemy).ToList();
        //creates a targetting ui for each enemy in range that can be seen
        foreach (GameObject enemy in enemyList)
        {
            if (!Physics2D.Linecast(transform.position, enemy.transform.position))
            {
                GameObject newIndicator = Instantiate(GameController.SelectionPrefab);
                newIndicator.transform.SetParent(MovementCanvas.transform);
                newIndicator.GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(enemy.transform.position);
            }
        }

    }


    //************************************************************************NOTE: make this specific to Agent at some point
    /// <summary>
    /// Determines which actions the character can take from it's current position
    /// </summary>
    /// <returns>A list of strings representing all possible actions</returns>
    protected override List<string> GetActions()
    {
        List<string> actionList = new List<string>();

        //checks to see if there are enemies in adjacent squares and adds "Melee" to the action list if so
        List<Vector3> adjacentSquares = new List<Vector3>
        {
            new Vector3(transform.position.x - 1, transform.position.y),
            new Vector3(transform.position.x + 1, transform.position.y),
            new Vector3(transform.position.x, transform.position.y + 1),
            new Vector3(transform.position.x, transform.position.y - 1)
        };
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (adjacentSquares.Contains(enemy.transform.position))
            {
                actionList.Add("Melee");
                break;
            }
        }

        //checks to see if there are enemies within line of sight and max range
        foreach (GameObject enemy in enemies)
        {
            if (!Physics2D.Linecast(transform.position, enemy.transform.position) && (System.Math.Abs(transform.position.x - enemy.transform.position.x) + System.Math.Abs(transform.position.y - enemy.transform.position.y) <= AttackRange) && !adjacentSquares.Contains(enemy.transform.position) && rangedWeapon.Ammo > 0)
            {
                actionList.Add("Ranged");
                break;
            }
        }

        //checks if this character knows any abiliities and adds "Ability" if so
        if (abilityList.Count > 0)
        {
            actionList.Add("Ability");
        }

        //checks if this character knows any spells and adds "Spell" if so
        if (spellList.Count > 0)
        {
            actionList.Add("Spell");
        }

        //"End" vs "Defend" ??
        actionList.Add("End");

        return actionList;
    }


    /// <summary>
    /// Allows the character to make a melee attack
    /// </summary>
    private IEnumerator Melee()
    {
        waitingForAction = false; //this variable refers only to the main action menu

        #region UI set-up
        //removes the action menu before bringing up the next one
        Destroy(UICanvas);
        //instantiates a canvas to display the action menu on
        UICanvas = Instantiate(GameController.CanvasPrefab);
        //creates a list of Vector3's with attackable targets
        List<Vector3> adjacentSquares = new List<Vector3>
        {
            new Vector3(transform.position.x - 1, transform.position.y),
            new Vector3(transform.position.x + 1, transform.position.y),
            new Vector3(transform.position.x, transform.position.y + 1),
            new Vector3(transform.position.x, transform.position.y - 1)
        };
        //creates a list of possible targets and a dictionary to hold the UI target icons based on their position
        List<Vector3> targets = (from gameObject in GameObject.FindGameObjectsWithTag("Enemy") where adjacentSquares.Contains(gameObject.transform.position) select gameObject.transform.position).ToList();
        Dictionary<Vector3, GameObject> targetIcons = new Dictionary<Vector3, GameObject>();
        foreach (Vector3 targetPos in targets)
        {
            targetIcons.Add(targetPos, null);
        }
        //creates UI for targetting
        for (int i = 0; i < targets.Count; i++)
        {
            targetIcons[targets[i]] = Instantiate(GameController.SelectionPrefab);
            targetIcons[targets[i]].transform.SetParent(UICanvas.transform);
            targetIcons[targets[i]].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(targets[i]);
        }
        //changes the icon on the first target "selection icon" to a "selected icon"
        int targetIndex = 0;
        Vector3 selectedPosition = targets[targetIndex];
        Destroy(targetIcons[selectedPosition]);
        targetIcons[selectedPosition] = Instantiate(GameController.SelectedPrefab);
        targetIcons[selectedPosition].transform.SetParent(UICanvas.transform);
        targetIcons[selectedPosition].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(selectedPosition);
        #endregion

        //waits while the user is selecting a target
        while (true)
        {
            yield return null;

            //changes which enemy is selected based on next and previous input and keeps track of the last selected position
            Vector3 lastSelectedPosition = selectedPosition;
            if (Input.GetButtonDown("Next"))
            {
                targetIndex++;
                if (targetIndex > targets.Count - 1) { targetIndex = 0; }
            }
            if (Input.GetButtonDown("Previous"))
            {
                targetIndex--;
                if (targetIndex < 0) { targetIndex = targets.Count - 1; }
            }
            selectedPosition = targets[targetIndex];

            //redraws the UI if the selected enemy changes
            if (lastSelectedPosition != selectedPosition)
            {
                //canvas.worldCamera = null; //camera must be removed and reassigned for new UI to render correctly
                //sets the previously selected square to have selection UI
                Destroy(targetIcons[lastSelectedPosition]);
                targetIcons[lastSelectedPosition] = Instantiate(GameController.SelectionPrefab);
                targetIcons[lastSelectedPosition].transform.SetParent(UICanvas.transform);
                targetIcons[lastSelectedPosition].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(lastSelectedPosition);
                //sets the newly selected square to have selected UI
                Destroy(targetIcons[selectedPosition]);
                targetIcons[selectedPosition] = Instantiate(GameController.SelectedPrefab);
                targetIcons[selectedPosition].transform.SetParent(UICanvas.transform);
                targetIcons[selectedPosition].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(selectedPosition);
            }

            //confirms attack target
            if (Input.GetButtonDown("Submit")) { break; }
            //returns to the previous action menu
            if (Input.GetButtonDown("Cancel"))
            {
                Destroy(UICanvas);
                ActionMenu();
                yield break;
            }
        }

        //removes UI as attack goes through
        Destroy(UICanvas);
        Destroy(MovementCanvas);

        //gets the enemy whose position matches the currently selected square
        CombatChar target = (from gameObject in GameObject.FindGameObjectsWithTag("Enemy") where gameObject.transform.position == selectedPosition select gameObject).ToList()[0].GetComponent<CombatChar>();

        //calculates damage to apply and calls TakeDamage()
        int damage = attack + meleeWeapon.Damage - target.Defense;
        target.BeginTakeDamage(damage);
        while (target.TakingDamage) { yield return null; }


        //allows TakeTurn to finish
        actionCompleted = true;
    }

    //damage calculation is off
    /// <summary>
    /// Allows the character to make a ranged attack
    /// </summary>
    private IEnumerator Ranged()
    {
        waitingForAction = false;

        #region UI set-up
        //removes the action menu before bringing up the next one
        Destroy(UICanvas);
        //instantiates a canvas to display the action menu on
        UICanvas = Instantiate(GameController.CanvasPrefab);
        //creates a list of possible targets and a dictionary to hold the UI target icons based on their position
        List<Vector3> targets = (from GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy") where System.Math.Abs((int)transform.position.x - enemy.transform.position.x) + System.Math.Abs((int)transform.position.y - enemy.transform.position.y) <= AttackRange select enemy.transform.position).ToList();
        Dictionary<Vector3, GameObject> targetIcons = new Dictionary<Vector3, GameObject>();
        foreach (Vector3 targetPos in targets)
        {
            targetIcons.Add(targetPos, null);
        }
        //creates UI for targetting
        for (int i = 0; i < targets.Count; i++)
        {
            targetIcons[targets[i]] = Instantiate(GameController.SelectionPrefab);
            targetIcons[targets[i]].transform.SetParent(UICanvas.transform);
            targetIcons[targets[i]].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(targets[i]);
        }
        //changes the icon on the first target "selection icon" to a "selected icon"
        int targetIndex = 0;
        Vector3 selectedPosition = targets[targetIndex];
        Destroy(targetIcons[selectedPosition]);
        targetIcons[selectedPosition] = Instantiate(GameController.SelectedPrefab);
        targetIcons[selectedPosition].transform.SetParent(UICanvas.transform);
        targetIcons[selectedPosition].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(selectedPosition);
        #endregion

        //waits while the user is selecting a target
        while (true)
        {
            yield return null;

            //changes which enemy is selected based on next and previous input and keeps track of the last selected position
            Vector3 lastSelectedPosition = selectedPosition;
            if (Input.GetButtonDown("Next"))
            {
                targetIndex++;
                if (targetIndex > targets.Count - 1) { targetIndex = 0; }
            }
            if (Input.GetButtonDown("Previous"))
            {
                targetIndex--;
                if (targetIndex < 0) { targetIndex = targets.Count - 1; }
            }
            selectedPosition = targets[targetIndex];

            //redraws the UI if the selected enemy changes
            if (lastSelectedPosition != selectedPosition)
            {
                //canvas.worldCamera = null; //camera must be removed and reassigned for new UI to render correctly
                //sets the previously selected square to have selection UI
                Destroy(targetIcons[lastSelectedPosition]);
                targetIcons[lastSelectedPosition] = Instantiate(GameController.SelectionPrefab);
                targetIcons[lastSelectedPosition].transform.SetParent(UICanvas.transform);
                targetIcons[lastSelectedPosition].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(lastSelectedPosition);
                //sets the newly selected square to have selected UI
                Destroy(targetIcons[selectedPosition]);
                targetIcons[selectedPosition] = Instantiate(GameController.SelectedPrefab);
                targetIcons[selectedPosition].transform.SetParent(UICanvas.transform);
                targetIcons[selectedPosition].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(selectedPosition);
            }

            //confirms attack target
            if (Input.GetButtonDown("Submit")) { break; }
            //returns to the previous action menu
            if (Input.GetButtonDown("Cancel"))
            {
                Destroy(UICanvas);
                ActionMenu();
                yield break;
            }
        }

        //removes UI as attack goes through
        Destroy(UICanvas);
        Destroy(MovementCanvas);

        //gets the enemy whose position matches the currently selected square
        CombatChar target = (from gameObject in GameObject.FindGameObjectsWithTag("Enemy") where gameObject.transform.position == selectedPosition select gameObject).ToList()[0].GetComponent<CombatChar>();

        //calculates damage to apply and calls TakeDamage()
        int damage = attack + rangedWeapon.Damage - target.Defense; //fix this calculation
        rangedWeapon.Ammo -= 1;
        target.BeginTakeDamage(damage);
        while (target.TakingDamage) { yield return null; }


        //allows TakeTurn to finish
        actionCompleted = true;
    }




















    //implement this
    /// <summary>
    /// Levels up this character
    /// </summary>
    protected override void LevelUp()
    {

    }
}
