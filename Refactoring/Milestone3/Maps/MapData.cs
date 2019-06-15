using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Team7GUI.Information;

namespace Team7GUI.Maps
{
    internal class MapData
    {
        private MapLayer maplayer = new MapLayer();
        private Border border;

        /// <summary>
        /// Determines what type is coming in and opens a map based on what it is
        /// </summary>
        /// <param name="element">Business or User</param>
        public MapData(BaseInfo element)
        {
            if (element is Business business)
            {
                BusinessMap(new List<Business>() { business });
            }
            else if (element is Users users)
            {
                UsersMap(users);
            }
        }

        /// <summary>
        /// Overloaded constructor to accept a list of businesses
        /// </summary>
        /// <param name="businesses">List of selected businesses</param>
        public MapData(List<Business> businesses)
        {
            BusinessMap(businesses);
        }

        /// <summary>
        /// Opens a map centered on a business with a textblock of information about the business
        /// </summary>
        /// <param name="businesses">List of businesses selected, 1 to many</param>
        private void BusinessMap(List<Business> businesses)
        {
            if (businesses.Count > 0)
            {
                Map popupMap = new Map
                {
                    Center = new Location(businesses[0].Latitude, businesses[0].Longitude),
                    Name = "Business"
                };

                foreach (Business currentBusiness in businesses)
                {
                    Location center = new Location(currentBusiness.Latitude, currentBusiness.Longitude);
                    popupMap.Children.Add(AddPushpin(currentBusiness, center));
                }
                popupMap.Children.Add(maplayer);
                CreatePopup(popupMap);
            }
        }

        /// <summary>
        /// Opens a map at the users last recorded Lat/Long coordinates
        /// </summary>
        /// <param name="users">The user selected</param>
        private void UsersMap(Users users)
        {
            Location center = new Location(users.Latitude, users.Longitude);

            Map popupMap = new Map
            {
                Center = center,
                Name = "User"
            };

            popupMap.Children.Add(AddPushpin(users, center));

            popupMap.Children.Add(maplayer);
            CreatePopup(popupMap);
        }

        /// <summary>
        /// Display the map as a popup with proper information
        /// </summary>
        /// <param name="map">The configured map</param>
        private void CreatePopup(Map map)
        {
            map.CredentialsProvider = new ApplicationIdCredentialsProvider("ArVh14HJTS5NYlLej7SaUdGNP0YR6zMVjMizCx4S-qpwo_nmK9sBvHsH3xfwXqau");
            map.ZoomLevel = 14;

            Popup mapPopup = new Popup
            {
                PlacementRectangle = new Rect(new Size(SystemParameters.FullPrimaryScreenWidth, SystemParameters.FullPrimaryScreenHeight)),
                StaysOpen = false,
                Height = 540,
                Width = 960,
                Placement = PlacementMode.Center,
                Child = map,
                PopupAnimation = PopupAnimation.Fade,
                IsOpen = true
            };

            // Only for business to hide the Business Info box when user zooms out far enough
            if (map.Name == "Business")
            {
                map.ViewChangeOnFrame += new EventHandler<MapEventArgs>(ThisMap_ViewChangeOnFrame);
            }
        }

        /// <summary>
        /// When user zooms out enough, the information box disappears
        /// </summary>
        /// <param name="sender">Map</param>
        /// <param name="e">N/A</param>
        private void ThisMap_ViewChangeOnFrame(object sender, MapEventArgs e)
        {
            if (sender is Map map)
            {
                if (maplayer.Children.Contains(border))
                {
                    if (map.ZoomLevel < 11)
                    {
                        border.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        border.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a pushpin to the map
        /// </summary>
        /// <param name="info">Business or User</param>
        /// <param name="center">Latitude and Longitude location</param>
        /// <returns>Pushpin with information embedded</returns>
        private Pushpin AddPushpin(BaseInfo info, Location center)
        {
            Pushpin pushpin = new Pushpin()
            {
                Location = center,
                Tag = info
            };

            pushpin.MouseDown += PinClicked;

            return pushpin;
        }

        /// <summary>
        /// Pushpin clicked
        /// </summary>
        /// <param name="sender">Pushpin</param>
        /// <param name="e">N/A</param>
        private void PinClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Pushpin p = sender as Pushpin;
            BaseInfo m = (BaseInfo)p.Tag;
            border = InfoBlock.Build(m);

            if (maplayer.Children.Count > 0)
            {
                ResetMapLayer();
            }
            else
            {
                ResetMapLayer();
                maplayer.AddChild(border, p.Location, new Point(10, 10));
            }

            // Remove the border/textblock from the maplayer
            void ResetMapLayer()
            {
                if (border.Parent != null)
                {
                    MapLayer parent = border.Parent as MapLayer;
                    if (parent.Children.Contains(border))
                    {
                        parent.Children.Clear();
                    }
                }
            }
        }
    }
}