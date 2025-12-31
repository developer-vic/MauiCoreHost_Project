namespace BiblCalCore
{
    /// <summary>
    /// Documentation and help text for the Biblical Calendar application
    /// Adapted from modDocumentation.cs
    /// </summary>
    public static class Documentation
    {
        public const string Version = "1.0";

        /// <summary>
        /// Gets documentation text for a specific mode
        /// </summary>
        public static string GetDocumentation(string mode)
        {
            return mode.ToLower() switch
            {
                "holydays" => GetHolyDaysDocumentation(),
                "localmoons" => GetLocalMoonsDocumentation(),
                "sunset" => GetSunsetDocumentation(),
                "jordan" => GetJordanDocumentation(),
                "flood" => GetFloodDocumentation(),
                "creation" => GetCreationDocumentation(),
                "golgotha" => GetGolgothaDocumentation(),
                "times" => GetTimesDocumentation(),
                "rabbinic" => GetRabbinicDocumentation(),
                "easter" => GetEasterDocumentation(),
                "conversions" => GetConversionsDocumentation(),
                _ => GetGeneralDocumentation()
            };
        }

        private static string GetHolyDaysDocumentation()
        {
            return @"Calculated Biblical Calendar " + Version + @"

This software package has many uses, some of which will be briefly introduced here.

HOLY DAYS estimates the dates of our God's Holy Days by calculating which evening the crescent of the New Moon would first be visible in Jerusalem. The calculations follow the Biblical principles governing Jehovah's Calendar.

These calculations are not a replacement for direct observation of the crescent new moon, but are instead an aid to assist in its observation. The azimuth is measured in degrees clockwise from the South (90 is directly west). The setting times are for the evening of the given date, while the Holy Days begin on the evening before their given date.

Enter the year you wish to calculate (leading minus sign for BCE (BC) years).";
        }

        private static string GetLocalMoonsDocumentation()
        {
            return @"LOCAL MOON - VISIBLE NEW MOON " + Version + @"

The LOCAL MOON section calculates the New Moons for the coordinates in the 'Latitude' and 'Longitude' boxes, allowing the user to verify the visibility of the new moon crescent in their own area. The 'GMT offset' box allows the user to select the local time zone for the location entered.

Enter the year you wish to calculate (use a negative sign for BCE (BC) years) and click on the Crescent Moon button.

The coordinates and GMT for your location can be found in various Atlas, maps or online by typing the location name into Google Earth's Fly to command.";
        }

        private static string GetSunsetDocumentation()
        {
            return @"SUNSETS " + Version + @"

This section calculates the sunset times for the coordinates in the 'Latitude' and 'Longitude' boxes, providing the user with sunset times for the entire year for that location. The 'UTC/GMT offset' box allows the user to select the local time zone for the location entered. The setting times are not adjusted for daylight saving.

Enter the year (negative for BCE (BC) years) you wish to calculate and click on the Sunset button.";
        }

        private static string GetJordanDocumentation()
        {
            return @"THE JORDAN CROSSING " + Version + @"

This module calculates Passover (Abib 14) and the Wave Offering for the years that the Israelites would have crossed the Jordan to occupy Canaan.

The Israelites first ate the food of the Promised Land on the day after Passover (Joshua 5:11). They were not allowed to do this until the Feast of the First-Fruit [the Wave Offering] was kept (Leviticus 23:9-14). As the Biblical Wave Offering always occurs on the day after the Sabbath during the Feast of Unleavened Bread, it follows that the Passover (Preparation Day) had to be the Sabbath (Sat) that year.

Enter the initial year for the run in the left text box and the final year of the run in the text box beside the Flood button. Click the Crossing button to start the run.

Bible chronology gives a probable crossing date of 1510 BCE (BC).";
        }

        private static string GetFloodDocumentation()
        {
            return @"THE WORLD-WIDE FLOOD " + Version + @"

This module calculates the number of days between the 17th day of the second month and the 17th day of the seventh month. We know from Genesis 7:11 and 8:3 that 'Noah's' flood lasted 150 days, extending from the 17th day of the second month until the 17th day of the seventh month. This module runs through a number of consecutive years and creates a table of the years that could have the required 150 days.

Enter the initial year for the run in the left text box and the final year of the run in the text box beside the Flood button. Click the Flood button to start the run.

Bible chronology places the year of the Flood near 2348 BCE (BC).";
        }

        private static string GetCreationDocumentation()
        {
            return @"CREATION DATES (ABIB 1) " + Version + @"

This section calculates the day of the week for Abib 1 to determine which years are possibly the Creation Year. Genesis 2:2&3, coupled with Exodus 20:11 confirm that the seventh day (Saturday in most of the world) is the Sabbath. This means that the Creation began on Sunday (ie, after sunset on Saturday evening). The module creates a table of the years that could have begun on Sunday.

Calculations are for 42 Deg East Long. and 38.5 Deg North Lat., which may be the approximate location of the Garden of Eden.

Bible chronology gives a probable creation year of 4004 BCE. (BC)";
        }

        private static string GetGolgothaDocumentation()
        {
            return @"JESUS CHRIST'S (JESHUA MESSIAH'S) DEATH AND RESURRECTION (GOLGOTHA) " + Version + @"

This module shows all the possible dates of Jesus Christ's death and resurrection. As the Scriptures make plain, Jeshua (Jesus) was crucified at Golgotha on the afternoon of Abib 14, the Preparation Day of the Passover Feast.

Jesus said there was one sign that would prove He was the Christ: He would spend three days and three nights in the heart of the earth, and then rise from the dead. (Matthew chap.12, verses 39 & 40)

The program shows that in the year 31 C.E.(A.D.), the year that history, combined with the Bible, confirms as the year of His cruel death, the Passover was on Wednesday.

Enter the initial year for the run in the left text box and the final year of the run in the text box beside the Cross button. Click the Cross button to start the run.";
        }

        private static string GetTimesDocumentation()
        {
            return @"SUN AND MOON TIMES " + Version + @"

TIMES calculates the times of sunrise, sunset, moonrise, moonset and illuminated fraction for the coordinates in the 'Latitude' and 'Longitude' boxes, providing the user with these times for the entire year. The 'UTC/GMT offset' box allows the user enter the locality's time zone. The times are not adjusted for daylight saving.

Enter the year you wish to calculate (negative for BCE (BC) years) and click on the Sundial button.";
        }

        private static string GetRabbinicDocumentation()
        {
            return @"Rabbinical Calculated Calendar " + Version + @"

The Rabbinic module calculates the months and Holy Days using the standard Rabbinical calendar as defined by Hillel II in the fourth century AD and reiterated by Maimonides in the middle ages. It uses their various unbiblical postponements and decisions for the timing of their months and holy days.

Enter the Gregorian year you wish to calculate (negative for BCE (BC) years) and click on the Compute button.";
        }

        private static string GetEasterDocumentation()
        {
            return @"Easter Dates " + Version + @"

Easter calculates the dates for the misnamed 'Good Friday' and 'Easter Sunday', using the Roman Catholic method. Please note that these calculations are only provided for comparison purposes. These are fake holy days that are intended to deflect Christians from keeping Jehovah God's real Holy Days as taught in the Bible.

Enter the year (negative for BCE (BC) years) you wish to calculate and click on the Compute button. The dates for 60 years from the date you entered will be calculated.";
        }

        private static string GetConversionsDocumentation()
        {
            return @"Calendar Conversions " + Version + @"

CONVERSIONS displays the date on four different calendars simultaneously. The date can be altered on any of the calendars and on clicking Compute, all of the calendars will be syncronised. The Julian Day Number is also displayed.

The Rabbinical calendar uses the standard Jewish format, as noted in the separate documentation for their calendar.

Pressing F1 will bring up the help screen for this module (and the others too).";
        }

        private static string GetGeneralDocumentation()
        {
            return @"Calculated Biblical Calendar " + Version + @"

This software package calculates Biblical calendar dates and astronomical times based on the visibility of the new moon crescent in Jerusalem.

For more information about specific modules, please select a mode from the menu.";
        }
    }
}

