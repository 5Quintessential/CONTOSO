using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DPE.ReferenceApps.Food.Client.ViewModels
{
    public class TeamViewModel : Lib.Common.BindableBase
    {
        #region Bindable Properties

        private List<TeamMemberViewModel> _team = new List<TeamMemberViewModel>()
        {
            new TeamMemberViewModel() 
            {
                Key = "Harry",
                Name = "Harry Mower",
                ImagePath = "/Assets/Team/Harry.jpg"
            },            
            new TeamMemberViewModel() 
            {
                Key = "Ian",
                Name = "Ian N. Bennett",
                ImagePath = "/Assets/Team/Ian.jpg"
            },            
            new TeamMemberViewModel() 
            {
                Key = "Jennifer",
                Name = "Jennifer Marsman",
                ImagePath = "/Assets/Team/Jennifer.jpg"
            },            
            new TeamMemberViewModel() 
            {
                Key = "Jerry",
                Name = "Jerry Nixon",
                ImagePath = "/Assets/Team/Jerry.jpg"
            },            
            new TeamMemberViewModel() 
            {
                Key = "Jim",
                Name = "Jim Blizzard",
                ImagePath = "/Assets/Team/Jim.jpg"
            },            
            new TeamMemberViewModel() 
            {
                Key = "Jit",
                Name = "Jit Ghosh",
                ImagePath = "/Assets/Team/Jit.jpg"
            }
        };

        public List<TeamMemberViewModel> Team
        {
            get { return this._team; }
        }

        private TeamMemberViewModel _currentMember;
        public TeamMemberViewModel CurrentMember
        {
            get { return this._currentMember; }
            set { this.SetProperty<TeamMemberViewModel>(ref this._currentMember, value); }
        }

        #endregion Bindable Properties
    }

    public class TeamMemberViewModel : Lib.Common.BindableBase
    {
        #region Bindable Properties

        private string _key;
        public string Key
        {
            get { return this._key; }
            set { this.SetProperty<string>(ref this._key, value); }
        }

        private string _name;
        public string Name
        {
            get { return this._name; }
            set { this.SetProperty<string>(ref this._name, value); }
        }

        private string _imagePath;
        public string ImagePath
        {
            get { return this._imagePath; }
            set { this.SetProperty<string>(ref this._imagePath, value); }
        }


        #endregion Bindable Properties
    }
}
