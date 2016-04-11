namespace Zrpg.Game

open System

module GameRunner =
  type Msg =
    | Tick
    | Kill

  type Game (state) =
    let rand = Random()
    let state:GameState = state

    let resolveCharacterBored (char:Character) =
      // This is a bored character with no events to be had.
      // For now, have the character mope around.
      state.CharLog char "I'm so bored!"

    let resolveCharacterLFG (char:Character) =
      // This character has no current events. We need to find him a quest!
      // First, get the current zone the character is in.
      let zone = state.GetZone char.zoneId

      // Check to see what npcs are in the area, maybe they have some quests.
      let npcs = zone.npcs |> Set.toList |> List.map state.GetNpc
      match npcs with
      | [] ->
        // No npcs? Generate a random action for this character.
        resolveCharacterBored char
      | npcs ->
        let npc =
          if npcs.Length = 0 then None
          else
            let i = (float npcs.Length * rand.NextDouble()) |> int
            Some npcs.[i]
        // Grab the first npc with quests available.
        // let npc = npcs |> List.tryFind (fun npc -> npc.events |> Set.isEmpty |> not)
        match npc with
        | None ->
          // No npcs with any quests?
          resolveCharacterBored char

        | Some npc ->
          // This npc does have quests, choose one.
          let events =
            npc.events |>
            Set.toList |>
            List.map state.GetEvent

          match events with
          | [] ->
            // No events?!
            resolveCharacterBored char

          | events ->
            let event =
              let i = float events.Length * rand.NextDouble() |> int
              events.[i]

            match event with
            | DialogueEvent dialogue ->
              // More chit chat?
              state.SetCharacter { char with eventId = Some dialogue.id }
              state.CharLogDialogue char dialogue

            | QuestEvent quest ->
              // Advance the character to this event.
              state.SetCharacter { char with eventId = Some quest.id }
              state.CharLogQuest char quest

    let resolveCharacterEvent (char:Character) =
      // Check to see if the character is in the middle of an event.
      match char.eventId with
      | None ->
        resolveCharacterLFG char

      | Some eventId ->
        // Get successive events and branch.
        let childEvents = state.GetChildEvents eventId

        // Choose one of the events to follow.
        // Take the first one for now.
        match childEvents with
        | [] ->
          resolveCharacterLFG char

        | event::_ ->
          match event with
          | DialogueEvent dialogue ->
            state.SetCharacter { char with eventId = Some dialogue.id }
            state.CharLogDialogue char dialogue
          
          | QuestEvent quest ->
            state.SetCharacter { char with eventId = Some quest.id }
            state.CharLogQuest char quest


    // This will advance the state of the game.
    let handleTick () =
      // First, get all characters.
      state.GetCharacters() |> Map.map (fun _ char ->
        resolveCharacterEvent char
      ) |> ignore
      // For each character, 
      ()

    let agent = MailboxProcessor<Msg>.Start(fun inbox ->
      let rec loop () = async {
        let! msg = inbox.Receive()

        try
          match msg with
          | Tick ->
            handleTick ()
          | Kill ->
            return () // Will stop the task
        with e ->
          printfn "%A" e

        return! loop ()
      }
      loop()
    )

    member this.Stop () =
      agent.Post Kill

    member this.Tick () =
      agent.Post Tick

