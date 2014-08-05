namespace Volcano
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using Volcano.Model;

    public class EditorContext : INotifyPropertyChanged
    {
        ProjectCache cache;
        string chunkFilter = String.Empty;
        ChunkTemplate currentChunk;
        MapObject currentObject;
        MapTool currentTool = MapTool.PaintChunk;
        ObservableCollection<ChunkTemplate> filteredChunkList = new ObservableCollection<ChunkTemplate>();
        UltimaProject project;
        int zLimit = 16;

        public EditorContext()
        {
            this.project = new UltimaProject { GameDirectory = @"c:\src\island\games\si" };
            this.project.Load();
            FilterChunkList();
        }
        
        public UltimaProject Project
        {
            get { return this.project; }
            set 
            { 
                this.project = value;
                this.cache = new ProjectCache { Project = this.project };
                Notify("Project");
                Notify("Cache");

                FilterChunkList();
                CurrentChunk = null;
            }
        }

        public ProjectCache Cache
        {
            get
            {
                if (this.cache == null)
                {
                    this.cache = new ProjectCache { Project = this.project };
                }
                return this.cache;
            }
        }

        public string ChunkFilter
        {
            get { return this.chunkFilter; }
            set 
            { 
                this.chunkFilter = value;
                Notify("ChunkFilter");
                FilterChunkList();
            }
        }

        public ChunkTemplate CurrentChunk
        {
            get { return this.currentChunk; }
            set 
            { 
                this.currentChunk = value; 
                Notify("CurrentChunk"); 
            }
        }

        public MapObject CurrentObject
        {
            get { return this.currentObject; }
            set
            {
                this.currentObject = value;
                Notify("CurrentObject");
            }
        }

        public MapTool CurrentTool
        {
            get { return this.currentTool; }
            set { this.currentTool = value; Notify("CurrentTool"); }
        }

        public IList<ChunkTemplate> FilteredChunkList { get { return this.filteredChunkList; } }

        public int ZLimit
        {
            get { return this.zLimit; }
            set { this.zLimit = value; Notify("ZLimit"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void FilterChunkList()
        {
            // Find the new set of chunks that match the filter; run through the chunk list and make it match.
            // TODO: Wow, this code sucks.
            //
            string[] keywords = this.chunkFilter.Split();
            var results = new HashSet<ChunkTemplate>(Cache.LookupChunks(keywords[0]));
            for (int i = 1; i < keywords.Length; i++)
            {
                results.IntersectWith(Cache.LookupChunks(keywords[i]));
            }

            // I'm too tired to build a diff apply thingy tonight.
            this.filteredChunkList.Clear();
            foreach (ChunkTemplate t in results) { this.filteredChunkList.Add(t); }
        }

        void Notify(string property)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        }
    }
}