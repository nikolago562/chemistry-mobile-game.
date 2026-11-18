using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ChemistryGame;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class LabExpansionChecks
{
    private static LabGame game;
    private static LabResearchUI research;
    private static Action next;
    private static double due;
    private static EnterPlayModeOptions oldOptions;
    private static string oldDirectory;
    private static readonly List<string> errors=new List<string>();
    private static readonly BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
    private static int before;
    public static void Run()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play mode before isolated checks.");
        errors.Clear();oldOptions=EditorSettings.enterPlayModeOptions;oldDirectory=GameSaveManager.TestDirectory;
        EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
        GameSaveManager.TestDirectory=Path.GetFullPath("Logs/expansion-qa-profile");Directory.CreateDirectory(GameSaveManager.TestDirectory);
        var data=new GameSaveData{profileCompleted=true,playerName="მკვლევარი",tutorialCompleted=true,starterCoatCollected=true,chemCoins=2000};
        data.ownedLabItems=PersonalLab.Catalog.Select(c=>c.ID).ToList();
        for(int i=0;i<7;i++)data.labPlacements.Add(new LabPlacement{itemID=PersonalLab.Catalog[i].ID,column=i<4?i:i==4?0:i==5?1:2,row=i<4?0:i==4?1:2});
        File.WriteAllText(Path.Combine(GameSaveManager.TestDirectory,"chem-lab.json"),JsonUtility.ToJson(data));
        Application.logMessageReceived+=Observe;EditorApplication.update+=Tick;EditorApplication.playModeStateChanged+=Restore;
        Schedule(Begin,1.5);EditorApplication.isPlaying=true;
    }
    private static void Restore(PlayModeStateChange change)
    {
        if(change!=PlayModeStateChange.EnteredEditMode)return;
        GameSaveManager.TestDirectory=oldDirectory;EditorSettings.enterPlayModeOptions=oldOptions;EditorApplication.playModeStateChanged-=Restore;
    }
    private static void Observe(string text,string stack,LogType type){if((type==LogType.Exception||type==LogType.Error)&&!stack.Contains("UnityEditor.Search")&&!text.Contains("FMOD"))errors.Add(text);}
    private static void Schedule(Action action,double wait){next=action;due=EditorApplication.timeSinceStartup+wait;}
    private static void Tick(){if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<due)return;try{var step=next;next=null;step?.Invoke();}catch(Exception e){Finish(e.ToString());}}
    private static void Ensure(bool value,string message){if(!value)throw new Exception(message);}
    private static object Field(object owner,string name)=>owner.GetType().GetField(name,Hidden).GetValue(owner);
    private static void Set(object owner,string name,object value)=>owner.GetType().GetField(name,Hidden).SetValue(owner,value);
    private static void UpdateResearch()=>typeof(LabResearchUI).GetMethod("Update",Hidden).Invoke(research,null);
    private static void Snapshot(string name)
    {
        // Capture the settled layout even when the first Editor frame is delayed.
        foreach(var w in UnityEngine.Object.FindObjectsByType<LabWindow>())
            typeof(LabWindow).GetField("opened",Hidden).SetValue(w,Time.unscaledTime-1);
        typeof(LabGameChecks).GetMethod("Snapshot",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{name});
    }
    private static void Begin()
    {
        game=UnityEngine.Object.FindAnyObjectByType<LabGame>();Ensure(game!=null,"Lab scene");
        var save=GameSaveManager.Instance;
        Ensure(game.Personal.UsableSlots==10,"Two entrance cells remain clear");
        Ensure(save.Data.ownedLabItems.Count==PersonalLab.Catalog.Length && save.Data.labPlacements.Count==7,"Saved furniture is retained during entrance migration");
        Ensure(!save.Data.labPlacements.Any(p=>game.Personal.IsEntranceCell(p.column,p.row)),"No furniture in the doorway");
        Ensure(save.Data.chemCoins==2000 && !game.Personal.Place("molecule",1,2),"Clearing the landing is free and reserved cells reject furniture");
        var door=game.Personal.Room.Find("გასასვლელი კარი");
        Ensure(door!=null && Mathf.Abs(door.localPosition.y-game.Personal.BackWallY)<0.01f && door.GetComponent<SpriteRenderer>().sprite==LaboratoryArt.LabDoor,"Door is seated in the raised wall");
        var feet=game.player.GetComponent<BoxCollider2D>();
        for(int step=0;step<=12;step++)
        {
            Vector2 point=Vector2.Lerp(game.Personal.SpawnPosition,game.Personal.ExitPosition,step/12f)+feet.offset;
            Ensure(!Physics2D.OverlapBoxAll(point,feet.size,0).Any(c=>!c.isTrigger && c.attachedRigidbody!=feet.attachedRigidbody),"Entrance path is physically clear");
        }
        typeof(LabGameChecks).GetMethod("SnapshotRoom",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{"26-personal-lab-walls",game.Personal.RoomOrigin+new Vector2(0.5f,3),10.5f,1280,1000});
        save.Load();Ensure(save.Data.labPlacements.Count==7 && !save.Data.labPlacements.Any(p=>game.Personal.IsEntranceCell(p.column,p.row)),"Furniture migration persists");
        save.Data.labPlacements.Clear();save.Data.ownedLabItems.Clear();save.Commit();
        typeof(PersonalLab).GetMethod("BuildEquipment",Hidden).Invoke(game.Personal,null);
        Ensure(ChemistryChallenges.General.Length>=16,"General quiz bank");
        foreach(var q in ChemistryChallenges.General)Ensure(q.Choices.Length==4 && q.Feedback.Length==4 && q.Choices.Distinct().Count()==4,"Quiz options and explanations: "+q.ID);
        foreach(var source in game.questions)
        {
            var q=ChemistryChallenges.Equation(source);Ensure(q.Choices.Length==4 && q.Choices.Distinct().Count()==4,"Four unique equation options");
            Ensure(ChemistryChallenges.ExplainMismatch(source.equationPrompt,q.Choices[0])=="ატომების რაოდენობა ემთხვევა.","Authored equation balance: "+source.questionID);
            for(int i=1;i<4;i++)Ensure(q.Feedback[i].Contains("მარცხნივ"),"Specific wrong-answer atom count");
        }
        game.Challenges.Open();Schedule(Home,0.4);
    }
    private static void Home()
    {
        Ensure(LabUI.AnyOpen && !game.player.canMove,"Challenge movement lock");Snapshot("20-challenge-menu");
        game.Challenges.StartRound(false);before=GameSaveManager.Instance.Data.chemCoins;
        game.Challenges.Choose(1);Ensure(GameSaveManager.Instance.Data.chemCoins==before,"Wrong answers cannot pay");
        Ensure(((TMP_Text)Field(game.Challenges,"feedback")).text.Contains(game.Challenges.Current.Feedback[1]),"Wrong-choice explanation");
        game.Challenges.UseHint();game.Challenges.Choose(0);Ensure(GameSaveManager.Instance.Data.chemCoins==before+6,"Hint/retry reward");
        game.Challenges.Choose(0);Ensure(GameSaveManager.Instance.Data.chemCoins==before+6,"No duplicate solve reward");Schedule(Quiz,0.4);
    }
    private static void Quiz(){Snapshot("21-general-quiz");game.Challenges.StartRound(true);game.Challenges.Choose(1);Schedule(Equation,0.4);}
    private static void Equation()
    {
        Snapshot("22-equation-explanation");game.Challenges.Choose(0);game.Challenges.Close();
        var save=GameSaveManager.Instance;
        foreach(var id in new[]{"membrane","chromatography","gas"})
        {
            int coins=save.Data.chemCoins;var item=PersonalLab.Catalog.Single(c=>c.ID==id);
            Ensure(game.Personal.Buy(id) && save.Data.chemCoins==coins-item.Price,"New equipment purchase "+id);
            Ensure(!game.Personal.Buy(id),"No duplicate equipment charge");
        }
        for(int i=0;i<3;i++)Ensure(game.Personal.Place(new[]{"membrane","chromatography","gas"}[i],i,0),"New table placement");
        save.Load();Ensure(save.Data.ownedLabItems.Contains("membrane") && save.Data.labPlacements.Count==3,"Equipment persistence");
        game.Personal.Activities.Open("membrane");research=game.GetComponent<LabResearchUI>();before=save.Data.chemCoins;
        Ensure(research.RightCount==6,"Membrane initial distribution");Schedule(Membrane,0.5);
    }
    private static void Membrane()
    {
        Ensure(research.RightCount==6,"Closed channel blocks crossing");Snapshot("23-membrane");
        for(int i=0;i<6;i++)Ensure(research.Pump(),"ATP pump step");
        Ensure(research.Pumped==6 && GameSaveManager.Instance.Data.chemCoins==before+20,"Six ATP pump reward");
        Ensure(!research.Pump(),"ATP cannot go negative");research.ResetMembrane();
        for(int i=0;i<6;i++)research.Pump();Ensure(GameSaveManager.Instance.Data.chemCoins==before+20,"Reset cannot duplicate research payout");
        research.ResetMembrane();Set(research,"channel",true);
        var particles=(IList)Field(research,"particles");
        // Force a small controlled crossing through the open channel, then exercise the real update.
        for(int i=0;i<11;i++){var p=particles[i];p.GetType().GetField("Position").SetValue(p,new Vector2(60+i,0));}
        UpdateResearch();Ensure(GameSaveManager.Instance.Data.chemCoins==before+40,"Diffusion milestone reward");
        research.Close();game.Personal.Activities.Open("chromatography");research.Predict(1);research.StartSeparation();Set(research,"runTime",7.99999f);UpdateResearch();
        Ensure(!GameSaveManager.Instance.Data.researchClaims.Contains("chromatography"),"Wrong chromatography prediction cannot pay");
        research.Predict(2);research.StartSeparation();Schedule(Chromatography,3.0);
    }
    private static void Chromatography()
    {
        Snapshot("24-chromatography");
        var bands=(List<RectTransform>)Field(research,"bands");Ensure(bands[2].anchoredPosition.y>bands[0].anchoredPosition.y,"Animated band separation");
        Set(research,"runTime",7.99999f);UpdateResearch();Ensure(GameSaveManager.Instance.Data.researchClaims.Contains("chromatography"),"Correct prediction reward");
        research.Close();game.Personal.Activities.Open("gas");
        var slider=UnityEngine.Object.FindObjectsByType<Slider>().Single(s=>s.name=="მოცულობა V");slider.value=1;
        Ensure(Mathf.Abs(research.RelativePressure-2)<0.001f,"Boyle law ratio");Schedule(Gas,2.4);
    }
    private static void Gas()
    {
        Snapshot("25-gas-piston");Ensure(GameSaveManager.Instance.Data.researchClaims.Contains("gas_compression"),"Stable pressure goal reward");
        var save=GameSaveManager.Instance;int coins=save.Data.chemCoins;save.Load();
        Ensure(save.Data.researchClaims.Count==4 && !save.ClaimResearch("gas_compression") && save.Data.chemCoins==coins,"Persistent once-daily rewards");
        save.Data.researchDay=GameSaveManager.Iso(DateTime.UtcNow.Date.AddDays(-1));
        Ensure(save.ClaimResearch("gas_compression") && save.Data.chemCoins==coins+20,"UTC research reset");
        Ensure(errors.Count==0,"Runtime errors: "+string.Join(";",errors));
        research.Close();
        Ensure(game.Personal.Travel(false),"Enter personal lab before door cooldown check");
        game.player.GetComponent<Rigidbody2D>().position=game.Personal.ExitPosition-game.player.GetComponent<BoxCollider2D>().offset;
        Schedule(DoorCooldown,1.3);
    }
    private static void DoorCooldown()
    {
        Ensure(game.Personal.Inside,"Standing in the doorway during arrival cooldown cannot bounce the player back");
        game.player.GetComponent<Rigidbody2D>().position=game.Personal.SpawnPosition;
        Schedule(DoorReapproach,0.2);
    }
    private static void DoorReapproach()
    {
        game.player.GetComponent<Rigidbody2D>().position=game.Personal.ExitPosition-game.player.GetComponent<BoxCollider2D>().offset;
        Schedule(DoorExited,0.3);
    }
    private static void DoorExited()
    {
        Ensure(!game.Personal.Inside,"Approaching the door again returns the player to class");
        Ensure(errors.Count==0,"Runtime errors after room travel");
        Finish(null);
    }
    private static void Finish(string failure)
    {
        File.WriteAllText("Logs/expansion-checks-result.txt",failure ?? "PASS\n16 general questions, 26 balanced equations, unique choices and per-choice explanations, hints, correct/incorrect and duplicate payouts, movement lock, 3 equipment purchases/placement/persistence, closed membrane, ATP cost and reset, diffusion reward, chromatography prediction and animation, Boyle ratio and pressure goal, persistent daily caps and UTC reset.");
        Application.logMessageReceived-=Observe;EditorApplication.update-=Tick;EditorApplication.isPlaying=false;
        if(failure!=null)Debug.LogError(failure);else Debug.Log("LAB_EXPANSION_CHECKS_PASSED");
    }
}
