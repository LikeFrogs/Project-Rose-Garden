using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum NewTestClassStatus { WaitingForParentClass, Targeting }

public class NewTestClass : PlayerCharacter
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

    #region flow
    private NewTestClassStatus classStatus;

    //targeting stuff
    List<Vector3> targets;
    int targetIndex;
    GameObject selectedIcon;
    Vector3 selectedPosition;


    #endregion

    //not finished
    #region Equipment
    private Pistol rangedWeapon;
    private Sword meleeWeapon;



    #endregion

    /// <summary>
    /// Runs at startup; used for initialization
    /// </summary>
    private void Awake()
    {
        abilityList = new List<string>();
        spellList = new List<string>();

        rangedWeapon = new Pistol(5, 5, 3);
        meleeWeapon = new Sword(5);

        classStatus = NewTestClassStatus.WaitingForParentClass;
    }

    /// <summary>
    /// Runs once per frame and handles all of the players actions
    /// </summary>
    protected override void Update()
    {
        if(status != Status.WaitingForSubclass) { base.Update(); }

        if(classStatus == NewTestClassStatus.Targeting)
        {
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
                //sets the previously selected square to have selection UI
                targetIcons[lastSelectedPosition].SetActive(true);

                //sets the newly selected square to have selected UI
                selectedIcon.GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(selectedPosition);
            }

            //confirms the action
            if (Input.GetButtonDown("Submit"))
            {
                StartCoroutine("CompleteAttack");
            }
            //returns to the previous action menu
            if (Input.GetButtonDown("Cancel"))
            {
                classStatus = NewTestClassStatus.WaitingForParentClass;

                //reset UI to pre-targeting phase
                ResetTargetIcons();
                Destroy(selectedIcon);
                DrawTargets();

                //bring up the action menu again
                ActionMenu();
            }
        }
    }

    /// <summary>
    /// Resets all variables for the end of the turn
    /// </summary>
    protected override void End()
    {
        classStatus = NewTestClassStatus.WaitingForParentClass;

        base.End();
    }

    /// <summary>
    /// highlights all enemies targetable from this space
    /// </summary>
    protected override void DrawTargets()
    {
        //destroy no longer relevant UI
        ResetTargetIcons();

        //dont draw targets if out of ammo
        if (rangedWeapon.Ammo <= 0) { return; }

        //dynamically shows which enemies can be attacked from the character's current position
        //gets list of enemies in range
        List<GameObject> enemyList = (from GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy") where System.Math.Abs((int)transform.position.x - enemy.transform.position.x) + System.Math.Abs((int)transform.position.y - enemy.transform.position.y) <= AttackRange select enemy).ToList();

        //creates a targetting ui for each enemy in range that can be seen
        Vector2 bottom = Camera.main.WorldToScreenPoint(new Vector3(0, 0));
        Vector2 top = Camera.main.WorldToScreenPoint(new Vector3(0, .5f));
        Vector2 targetIconDimension = new Vector2(top.y - bottom.y, top.y - bottom.y);
        foreach (GameObject enemy in enemyList)
        {
            if (!Physics2D.Linecast(transform.position, enemy.transform.position))
            {
                targetIcons[enemy.transform.position] = unusedTargetIcons[unusedTargetIcons.Count - 1];
                unusedTargetIcons.RemoveAt(unusedTargetIcons.Count - 1);
                targetIcons[enemy.transform.position].SetActive(true);
                targetIcons[enemy.transform.position].GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(enemy.transform.position);
                targetIcons[enemy.transform.position].GetComponent<RectTransform>().sizeDelta = targetIconDimension;
            }
        }
    }

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
    /// Sets up a melee attack
    /// </summary>
    private void Melee()
    {
        status = Status.WaitingForSubclass;

        //deactivate action buttons
        ResetActionButtons();
        //creates a list of Vector3's with attackable targets
        List<Vector3> adjacentSquares = new List<Vector3>
        {
            new Vector3(transform.position.x - 1, transform.position.y),
            new Vector3(transform.position.x + 1, transform.position.y),
            new Vector3(transform.position.x, transform.position.y + 1),
            new Vector3(transform.position.x, transform.position.y - 1)
        };
        //creates a list of possible targets and a dictionary to hold the UI target icons based on their position
        targets = (from gameObject in GameObject.FindGameObjectsWithTag("Enemy") where adjacentSquares.Contains(gameObject.transform.position) select gameObject.transform.position).ToList();
        selectedIcon = Instantiate(GameController.SelectedPrefab);
        selectedIcon.transform.SetParent(canvas.transform);
        selectedIcon.SetActive(false);

        //changes the icon on the first target "selection icon" to a "selected icon"
        targetIndex = 0;
        selectedPosition = targets[targetIndex];
        targetIcons[selectedPosition].SetActive(false);
        selectedIcon.SetActive(true);
        selectedIcon.GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(selectedPosition);
        Vector2 bottom = Camera.main.WorldToScreenPoint(new Vector3(0, 0));
        Vector2 top = Camera.main.WorldToScreenPoint(new Vector3(0, .5f));
        Vector2 targetIconDimension = new Vector2(top.y - bottom.y, top.y - bottom.y);
        selectedIcon.GetComponent<RectTransform>().sizeDelta = targetIconDimension;

        classStatus = NewTestClassStatus.Targeting;
    }

    /// <summary>
    /// sets up a ranged attack
    /// </summary>
    private void Ranged()
    {
        status = Status.WaitingForSubclass;

        //deactivate action buttons
        ResetActionButtons();

        //creates a list of possible targets and a dictionary to hold the UI target icons based on their position
        targets = (from GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy") where System.Math.Abs((int)transform.position.x - enemy.transform.position.x) + System.Math.Abs((int)transform.position.y - enemy.transform.position.y) <= AttackRange select enemy.transform.position).ToList();
        selectedIcon = Instantiate(GameController.SelectedPrefab);
        selectedIcon.transform.SetParent(canvas.transform);
        selectedIcon.SetActive(false);

        //changes the icon on the first target "selection icon" to a "selected icon"
        targetIndex = 0;
        selectedPosition = targets[targetIndex];
        targetIcons[selectedPosition].SetActive(false);
        selectedIcon.SetActive(true);
        selectedIcon.GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(selectedPosition);
        Vector2 bottom = Camera.main.WorldToScreenPoint(new Vector3(0, 0));
        Vector2 top = Camera.main.WorldToScreenPoint(new Vector3(0, .5f));
        Vector2 targetIconDimension = new Vector2(top.y - bottom.y, top.y - bottom.y);
        selectedIcon.GetComponent<RectTransform>().sizeDelta = targetIconDimension;

        classStatus = NewTestClassStatus.Targeting;

    }

    /// <summary>
    /// Finishes an attack that was set up
    /// </summary>
    private IEnumerator CompleteAttack()
    {
        //removes UI as attack goes through
        ResetTargetIcons();
        Destroy(selectedIcon);

        //gets the enemy whose position matches the currently selected square
        CombatChar target = (from gameObject in GameObject.FindGameObjectsWithTag("Enemy") where gameObject.transform.position == selectedPosition select gameObject).ToList()[0].GetComponent<CombatChar>();

        //calculates damage to apply and calls TakeDamage()
        int damage = attack + rangedWeapon.Damage - target.Defense; //fix this calculation
        rangedWeapon.Ammo -= 1;
        target.BeginTakeDamage(damage);
        while (target.TakingDamage) { yield return null; }

        //finishes the turn
        End();
    }



    //implement this
    /// <summary>
    /// Levels up this character
    /// </summary>
    protected override void LevelUp()
    {

    }
}
