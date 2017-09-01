[<AutoOpen>]
module Infrastructure =
    open Argu

    type CliArguments =
        | List
        | Add of name:string * volume:int
        | Remove of id:int
        | Apply of id:int
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | List _ -> "List all stored profiles."
                | Add _ -> "Add a new profile with a name and audio volume."
                | Remove _ -> "Remove a profile by given id."
                | Apply _ -> "Apply a profile by given id."

[<AutoOpen>]
module Domain =
    let MinVolume = 0

    let MaxVolume = 100

    type AudioVolumeProfile =
        | Profile of id:int * name:string * volume:int
        | None

module Audio =
    open AudioSwitcher.AudioApi.CoreAudio

    let setVolume value =
        let defaultPlaybackDevice = CoreAudioController().DefaultPlaybackDevice
        defaultPlaybackDevice.Volume <- value
        
module Storage =
    open System
    open System.IO
    
    let private storageFile =
        Path.Combine(Environment.CurrentDirectory, "avm.profiles")

    let private createStorageFileIfNotExists() =
        if not(File.Exists storageFile) then
            use stream = File.Create storageFile 
            stream |> ignore

    let listProfiles =
        createStorageFileIfNotExists()
        File.ReadAllLines storageFile
        |> Array.map (fun line ->
                match line.Split ',' with
                | [| id; name; volume |] ->
                    match Int32.TryParse volume with
                    | true, value when value >= MinVolume && value <= MaxVolume -> Profile(Convert.ToInt32(id), name, value)
                    | _ -> failwith (sprintf "'%s' is not a valid value for audio volume" volume)
                | _ -> None)
        |> Array.toList

    let private findNextId() =
        let profiles = listProfiles
        if profiles.IsEmpty then
            1
        else
            profiles
            |> List.map (fun profile ->
                match profile with
                | Profile (id, _, _) -> id
                | None -> 0)
            |> List.max 
            |> (+) 1

    let addProfile name volume =
        createStorageFileIfNotExists()
        let nextId = findNextId()
        let entry = sprintf "%i,%s,%i" nextId name volume
        File.AppendAllLines(storageFile, entry |> Seq.singleton)

    let removeProfile profileId =
        createStorageFileIfNotExists()
        let remainingProfiles =
            listProfiles
            |> List.filter (fun profile ->
                match profile with
                | Profile(id, _, _) when id = profileId -> false
                | None -> false
                | _ -> true)
            |> List.map (fun profile ->
                match profile with
                | Profile(id, name, volume) -> sprintf "%i,%s,%i" id name volume
                | None -> "")
            |> List.filter (fun line ->
                String.IsNullOrWhiteSpace(line)
                |> not)

        File.WriteAllLines(storageFile, remainingProfiles)


[<EntryPoint>]
let main argv = 
    let parser = Argu.ArgumentParser.Create<CliArguments>()
    
    match parser.ParseCommandLine(argv).GetAllResults() with
    | [ argument ] ->
        match argument with
        | List ->
            let profiles = Storage.listProfiles
            if profiles.IsEmpty then
                printfn "No profile available."
            else
                profiles
                |> List.iter (fun profile ->
                    match profile with
                    | Profile (id, name, volume) -> printfn "Id: %i | Name: %s | Volume: %i" id name volume
                    | None -> ())
        | Add (name, volume) -> Storage.addProfile name volume
        | Remove id -> Storage.removeProfile id
        | Apply id ->
            let profiles = Storage.listProfiles
            if not(profiles.IsEmpty) then
                let valueToApply =
                    profiles
                    |> List.map (fun profile ->
                        match profile with
                        | Profile(profileId, _, volume) when profileId = id -> volume
                        | _ -> -1)
                    |> List.find (fun volume -> volume <> -1)
                Audio.setVolume(valueToApply |> float)
                printfn "Audio volume was set to %i" valueToApply
    | _ -> printfn "%s" (parser.PrintUsage())

    0
