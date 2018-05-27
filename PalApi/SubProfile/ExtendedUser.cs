using System;
using System.Collections.Generic;
using System.Text;

namespace PalApi.SubProfile
{
    using Types;
    using Parsing;

    /// <summary>
    /// Extends the user profile (Most of these are self explaintory there for uncommented)
    /// </summary>
    public class ExtendedUser : User, IParsable
    {
        /// <summary>
        /// Full name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MiddleNames { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Surname { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Language Language { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Sex Gender { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Relationship RelationshipStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public LookingFor LookingFor { get; set; }
        /// <summary>
        /// Date of Birth - Day
        /// </summary>
        public int DobDay { get; set; }
        /// <summary>
        /// Date of birth - Month
        /// </summary>
        public int DobMonth { get; set; }
        /// <summary>
        /// Date of birth - year
        /// </summary>
        public int DobYear { get; set; }
        /// <summary>
        /// Auto property for the Birthday
        /// </summary>
        public DateTime DateOfBirth => new DateTime(DobYear, DobMonth, DobDay);
        /// <summary>
        /// Generates the age from Date Of Birth
        /// </summary>
        public int Age => (int)Math.Ceiling((DateOfBirth - DateTime.Now).TotalDays / 365);
        /// <summary>
        /// 
        /// </summary>
        public string AboutMe { get; set; }
        /// <summary>
        /// Any tags from the users profile (phased out of new iOS and Android Apps)
        /// </summary>
        public List<string> Tags { get; set; }
        /// <summary>
        /// Any websites from the users profile (phased out of new iOS and Android Apps)
        /// </summary>
        public List<string> Websites { get; set; }

        /// <summary>
        /// Parses the information from the DataMap given by subprofile
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="map"></param>
        public new void Process(DataMap map)
        {
            if (map.ContainsKey("sub-id"))
                Id = int.Parse(map.GetValue("sub-id"));
            if (map.ContainsKey("name"))
                Name = map.GetValue("name");
            if (map.ContainsKey("name1"))
                FirstName = map.GetValue("name1");
            if (map.ContainsKey("name2"))
                MiddleNames = map.GetValue("name2");
            if (map.ContainsKey("name3"))
                Surname = map.GetValue("name3");
            if (map.ContainsKey("lang"))
                Language = (Language)int.Parse(map.GetValue("lang"));
            if (map.ContainsKey("sex"))
                Gender = (Sex)int.Parse(map.GetValue("sex"));
            if (map.ContainsKey("relstatus"))
                RelationshipStatus = (Relationship)int.Parse(map.GetValue("relstatus"));
            if (map.ContainsKey("after"))
                LookingFor = (LookingFor)int.Parse(map.GetValue("after"));
            if (map.ContainsKey("about"))
                AboutMe = map.GetValue("about");
            if (map.ContainsKey("dob_d"))
                DobDay = map.GetValueInt("dob_d");
            if (map.ContainsKey("dob_m"))
                DobMonth = map.GetValueInt("dob_m");
            if (map.ContainsKey("dob_y"))
                DobYear = map.GetValueInt("dob_y");
            base.Process(map);
        }

        /// <summary>
        /// Converts the current class to a DataMap for serialization.
        /// Needs to be nested in 2 other maps before sent off (FYI)
        /// user_data
        ///     ext
        ///         (this map)
        /// <see cref="Networking.PacketTemplates.SubProfileUpdate(ExtendedUser)"/>
        /// </summary>
        /// <returns>The datamap of the current user.</returns>
        public DataMap ToMap()
        {
            var map = new DataMap();
            map.SetValue("name", Name);
            map.SetValue("name1", FirstName);
            map.SetValue("name2", MiddleNames);
            map.SetValue("name3", Surname);
            map.SetValue("lang", (int)Language);
            map.SetValue("sex", (int)Gender);
            map.SetValue("relstatus", (int)RelationshipStatus);
            map.SetValue("after", (int)LookingFor);
            map.SetValue("about", AboutMe);
            map.SetValue("dob_d", DobDay);
            map.SetValue("dob_m", DobMonth);
            map.SetValue("dob_y", DobYear);
            //Excluding URLS and Tags since they're deprecated. (note to future self looking for map data)
            return map;
        }
    }
}
