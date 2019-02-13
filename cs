#!/bin/bash

########## COLOR TABLE ##########
BLACK='\033[0;30m'
DARKGRAY='\033[1;30m'
RED='\033[0;31m'
LIGHTRED='\033[1;31m'
GREEN='\033[0;32m'
LIGHTGREEN='\033[1;32m'
BROWNORANGE='\033[0;33m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
LIGHTBLUE='\033[1;34m'
PURPLE='\033[0;35m'
LIGHTPURPLE='\033[1;35m'
CYAN='\033[0;36m'
LIGHTCYAN='\033[1;36m'
LIGHTGRAY='\033[0;37m'
WHITE='\033[1;37m'
NC='\033[0m'
########## END COLOR TABLE ##########

########## GLOBAL VARIABLES ##########
project_name=${PWD##*/}
projects_folder=~/repos
project_prefix=$projects_folder/envs/$project_name
########## END GLOBAL VARIABLES ##########

########## FUNCTIONS ##########
function print_banner() # guess what
{
  printf -- "${BLUE}\n\n";
  printf -- " ,-----. ,-----. ,--.  ,--.,------.    ,---.       ,---.  ,--.   ,--.,--.,--------. ,-----.,--.  ,--. \n";
  printf -- "'  .--./'  .-.  '|  ,'.|  ||  .-.  \  /  O  \     '   .-' |  |   |  ||  |'--.  .--''  .--./|  '--'  | \n";
  printf -- "|  |    |  | |  ||  |' '  ||  |  \  :|  .-.  |    \`.  \`-. |  |.'.|  ||  |   |  |   |  |    |  .--.  | \n";
  printf -- "'  '--'\'  '-'  '|  | \`   ||  '--'  /|  | |  |    .-'    ||   ,'.   ||  |   |  |   '  '--'\|  |  |  | \n";
  printf -- " \`-----' \`-----' \`--'  \`--'\`-------' \`--' \`--'    \`-----' '--'   '--'\`--'   \`--'    \`-----'\`--'  \`--' \n";
  printf -- "                                                                                  Author: E.Claassens\n";
  printf -- "${NC}\n";
}

function box_out() # Draw a box around the text you pass
{
  local s=("$@") b w
  for l in "${s[@]}"; do
    ((w<${#l})) && { b="$l"; w="${#l}"; }
  done
  tput setaf 3
  echo "+-${b//?/-}-+
| ${b//?/ } |"
  for l in "${s[@]}"; do
    printf '| %s%*s%s |\n' "$(tput setaf 4)" "-$w" "$l" "$(tput setaf 3)"
  done
  echo "| ${b//?/ } |
+-${b//?/-}-+"
  tput sgr 0
}

function f_create_yml() # Create the environment.yml file
{
  printf -- "-> Creating environment.yml file\n"
  conda env export -p $project_prefix > environment.yml
  printf -- "   After adding packages update the environment file!\n"
  printf -- "-> ${YELLOW}conda env export -p $project_prefix > environment.yml${NC}.\n"
}

function f_activate_env()
{
  printf -- "-> Activating environment.\n"
  source activate $project_prefix
}

function f_create_new_env()
{
  PS3='Please enter your choice: '
  select opt in "Latest 3.x" "3.7" "3.6" "3.5" "Latest 2.x"; do
    case $opt in
      "Latest 2.x" ) pyver=2; break;;
      "Latest 3.x" ) pyver=3; break;;
      "3.5" ) pyver=3.5; break;;
      "3.6" ) pyver=3.6; break;;
      "3.7" ) pyver=3.7; break;;
      *) echo "invalid option $REPLY";;
    esac
  done

  conda create -p $project_prefix python=$pyver -y
}
########## END FUNCTIONS ##########

print_banner

# --help flag
if [ ${#@} -ne 0 ] && [ "${@#"--help"}" = "" ]
then
  printf -- "${BLUE}NEED HELP?\n"
  printf -- "${LIGHTBLUE} cs stands for conda switch environment. Just run this
  command from the git project folder ($projects_folder) and it will:\n
  - create conda environment if not exist\n
  - activate/deactivate the conda environment\n
  - create/update environment.yml${NC}\n\n"
  return 0
fi

# Check if executed as source
if [ "$0" = "$BASH_SOURCE" ]; then
    printf -- "${RED}Error: Script must be sourced (${YELLOW}Run as: source ${0##*/}${RED})\n"
    exit 1
fi
printf -- "\n"

#check if conda is installed
_=$(command -v conda);
if [ "$?" != "0" ]
then
  printf -- "${RED}You do not seem to have Conda installed.${NC}\n"
  printf -- "Go to https://swfactory.aegon.com/confluence/x/vEejAg for instructions.\n"
  printf -- "\n Bye\n\n"
  return 100
fi

# Check if you are in the repo folder
if [ "$PWD" = "$projects_folder/$project_name" ] && [ "$PWD" != "$projects_folder/envs" ]
then
  box_out "Project name: $project_name" "Project prefix: $project_prefix"
else
  printf -- "${RED}"
	printf -- "You are running the script from the wrong location!\n"
	printf -- "${NC}"
	printf -- " ->  Expecting $projects_folder/${YELLOW}<project name>${NC}\n"
	printf -- " ->  Found $PWD/\n\n"
	return 100
fi

# MAIN STATEMENT
if [ -n "$CONDA_DEFAULT_ENV" ]
then
  printf -- "Environment already activated ($CONDA_DEFAULT_ENV).\n"
  
  # Check if you are still in the right project folder
  if [ "$CONDA_DEFAULT_ENV" = "$project_name" ]
  then
    printf -- "\n${YELLOW}Do you want to update the environment.yml file?${NC}\n"
    PS3='Please enter your choice: '
    select opt in "Yes" "No"; do
        case $opt in
            Yes ) f_create_yml; break;;
            No ) break;;
        esac
    done

    printf -- "\n${YELLOW}Do you want to deactivate ($CONDA_DEFAULT_ENV)?${NC}\n"
    PS3='Please enter your choice: '
    select opt in "Yes" "No"; do
        case $opt in
            Yes ) conda deactivate; break;;
            No ) break;;
        esac
    done
  else
    printf -- "${RED}"
    printf -- "You are running the script from the wrong location!\n"
    printf -- "${NC}"
    printf -- " ->  Expecting $projects_folder/${YELLOW}$CONDA_DEFAULT_ENV${NC}\n"
    printf -- " ->  Found     $projects_folder/${YELLOW}$project_name${NC}\n"
    printf -- "\n"

    printf -- "\n${YELLOW}Do you want to deactivate ($CONDA_DEFAULT_ENV)?${NC}\n"
    PS3='Please enter your choice: '
    select opt in "Yes" "No"; do
        case $opt in
            Yes ) conda deactivate; break;;
            No ) break;;
        esac
    done
    return 0
  fi

else
  printf -- "Environment not activated.\n"
  # Check if env already exists
  if [ -d "$project_prefix" ] 
  then # ENVIRONMENT EXISTS
    printf -- "Conda environment exist "
    if [ -f "$projects_folder/$project_name/environment.yml" ]
    then # YML FOUND, ACTIVATE
      printf -- "with an environment.yml file\n"
      f_activate_env
    else # NO YML FOUND, CREATE AND ACTIVATE
      printf -- "without an environment.yml file\n"
      f_create_yml
      f_activate_env
    fi
  else # ENVIRONMENT DOES NOT EXIST
    printf -- "No Conda environment found\n"
    if [ -f "$projects_folder/$project_name/environment.yml" ]
    then
      printf -- "Found environment.yml\n"
      printf -- "-> Creating environment using environment.yml\n"
      conda env create -p $project_prefix -f environment.yml
      f_activate_env
    else
      printf -- "No environment.yml file found\n"
      printf -- "-> Creating a new environment. Choose Python version.\n"
      f_create_new_env
      f_create_yml
      f_activate_env
    fi
    printf -- "-> ${YELLOW}conda deactivate${NC} to deactivate the current environment.\n"
  fi
fi

printf -- "\n"
