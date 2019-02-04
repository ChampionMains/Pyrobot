﻿Vue.use(vueMoment);

var profileApiService = {
    state: {
        summoners: {},
        champions: {},
        subreddits: {},
        updatingSummoners: {}, // map from summonerId to lastUpdateTime.
        hiddenSummoners: {}, // map from summonerId to TRUE.
        updating: false,
        status: null
    },
    _pollTimeout: null,
    poll: function(i) {
        // default: polls 5 times with interval of 5 seconds (total ~20 seconds; fencepost counting)
        i = i === undefined ? 5 : i;
        clearTimeout(this._pollTimeout);
        this.state.updating = true;

        fetch('/profile/api/data')
            .then(function(res) {
                return res.json();
            })
            .then(function(data) {
                this.state.updating = false;
                Object.assign(this.state, data.result);
                // delay either 5 seconds or an hour
                var delay = (--i > 0) ? 5e3 : 3.6e6;
                this._pollTimeout = setTimeout(this.poll.bind(this), delay, i);
            }.bind(this))
            .catch(function(ex) {
                this.state.updating = false;
                console.log('updating data failed', ex);
                this.state.status = { error: 'Error updating summoners' };
            }.bind(this));
    },
    updateSummoner: function(summoner) {
        this.state.updatingSummoners[summoner.id] = summoner.lastUpdate;
        fetch('/profile/api/summoner/refresh', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(summoner),
            })
            .then(function(res) {
                this.poll();
//                // TODO cancel this timeout.
//                setTimeout(function() {
//                    delete this.state.updatingSummoners[summoner.id];
//                }.bind(this), 5000);
            }.bind(this))
            .catch(function(ex) {
                // TODO just pretend to update w/ failure message if failed.
                console.error('updating data failed', ex);
                this.state.status = { error: 'Error updating summoner' };
            }.bind(this));
    },
    removeSummoner: function(summoner) {
        this.state.hiddenSummoners[summoner.id] = true;
        fetch('/profile/api/summoner/delete', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(summoner),
            })
            .then(function(res) {
                Vue.delete(this.state.summoners, summoner.id);
            }.bind(this))
            .catch(function(ex) {
                Vue.delete(this.state.hiddenSummoners, summoner.id);
                console.error('deleting summoner failed', ex);
                this.state.status = { error: 'Error removing summoner' };
            }.bind(this));
    },
    isSummonerUpdating: function(summoner) {
        var updateTime = this.state.updatingSummoners[summoner.id]; // May be undefined.
        return summoner.lastUpdate === updateTime;
    },
    registerSummonerInfo: function(summonerModel) {
        return fetch('/profile/api/summoner/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(summonerModel)
            })
            .then(function(res) {
                return res.json();
            });
    },
    validateSummonerIcon: function(summonerModel) {
        var validationAttempts = 3;
        return (function tryValidate() {
            return fetch('/profile/api/summoner/validate',
                    {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(summonerModel)
                    })
                .then(function(res) {
                    if (res.ok)
                        return true;
                    if (res.status === 417) {
                        if (--validationAttempts) {
                            return Promise.delay(5000).then(tryValidate);
                        }
                        throw new Error('Validation failed. Please check your icon and try again.');
                    }
                    return res.json().then(function(data) {
                        throw new Error(data.error, JSON.stringify(data));
                    });
                });
        })();
    },
    updateSubredditFlair: function(flairModel) {
        return fetch('/profile/api/subreddit/update', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(flairModel)
            })
            .then(function(res) {
                this.poll();
            }.bind(this))
            .catch(function(ex) {
                this.poll();
                throw ex;
            }.bind(this));
    }
};

var modalService = {
    showDialog: function(dialog) {
        document.body.style.overflow = 'hidden';
        dialog.showModal();
    },
    closeDialog: function(dialog) {
        document.body.style.overflow = null;
        dialog.close();
    },
    // Call during `mounted()`.
    registerDialog: function(dialog) {
        if (!dialog.showModal)
            dialogPolyfill.registerDialog(dialog);
    }
};

var dialogRemoveSummonerApp = new Vue({
    el: '#modal-remove-summoner',
    data: {
        summonerToRemove: null
    },
    methods: {
        showDialog: function() {
            modalService.showDialog(this.$el);
        },
        closeDialog: function() {
            modalService.closeDialog(this.$el);
        },

        promptRemoveSummoner: function(summoner) {
            this.summonerToRemove = summoner;
            this.showDialog();
        },
        removeSummoner: function() {
            this.closeDialog();
            profileApiService.removeSummoner(this.summonerToRemove);
        }
    },
    mounted: function() {
        modalService.registerDialog(this.$el);
    }
});

var dialogAddSummonerApp = new Vue({
    el: '#modal-add-summoner',
    data: {
        busy: false,
        page: 1,
        summonerModel: {
            summonerName: null,
            region: null,
            token: null
        },
        summonerInfoError: null,
        summonerValidError: null,
        profileIcon: null,
        alert: null
    },
    methods: {
        showDialog: function() {
            modalService.showDialog(this.$el);
        },
        closeDialog: function() {
            modalService.closeDialog(this.$el);
        },

        promptAddSummoner: function() {
            this.showDialog();
        },
        cancel: function() {
            this.closeDialog();
            this.busy = false;
            this.page = 1;
            this.summonerModel.token = null;
            this.summonerInfoError = null;
            this.summonerValidError = null;
            this.profileIcon = null;
            this.alert = null;
        },
        nextStep: function() {
            this.busy = true;
            this.alert = null;

            if (!this.summonerModel.token) {
                // Step 1: post summoner info.
                profileApiService.registerSummonerInfo(this.summonerModel)
                    .then(function(data) {
                        if (data.error) {
                            console.log('Error getting summoner.', data);
                            this.summonerInfoError = data.error;
                            return;
                        }
                        this.summonerModel.token = data.result.token;
                        this.profileIcon = data.result.profileIcon;
                        setTimeout(function() {
                            this.busy = false;
                            this.page = 2;
                        }.bind(this), 500);
                    }.bind(this))
                    .catch(function(ex) {
                        this.busy = false;
                        console.error(ex);
                    });
            }
            else {
                // Step 2: validate summoner icon.
                this.summonerValidError = null;
                this.busy = true;
                profileApiService.validateSummonerIcon(this.summonerModel)
                    .then(function() {
                        this.cancel();
                        profileApiService.poll();
                    }.bind(this))
                    .catch(function(ex) {
                        console.log('Error validation', ex);
                        this.summonerValidError = ex.message;
                        this.busy = false;
                    }.bind(this)); //TODO
            }
        }
    },
    computed: {
        disableNext: function() {
            var model = this.summonerModel;
            return this.busy || !model.summonerName || !model.region;
        }
    },
    watch: {
        'summonerInfoError': function() {
            document.getElementById('summonerName').parentElement.className += ' is-invalid'; // TODO: use material check update fn.
        }
    },
    mounted: function() {
        modalService.registerDialog(this.$el);
    }
});

var dialogEditSubredditFlairApp = new Vue({
    el: '#modal-edit-subredditflair',
    data: {
        set: false,
        subredditData: {
            flair: {}
        },
        rankData: null,
        champData: null
    },
    methods: {
        showDialog: function() {
            modalService.showDialog(this.$el);
        },
        closeDialog: function() {
            modalService.closeDialog(this.$el);
        },

        promptEditSubredditFlair: function(subredditData, rankData, champData) {
            this.set = true;
            this.subredditData = JSON.parse(JSON.stringify(subredditData));
            this.rankData = rankData;
            this.champData = champData;
            this.showDialog();
        },
        saveSubredditFlair: function() {
            this.closeDialog();
            profileApiService.updateSubredditFlair(this.subredditData.flair);
        }
    },
    watch: {
        'subredditData.flair.rankEnabled': function() {
            setTimeout(function() {
                var el = document.getElementById('checkbox-rank-emblem');
                if (el) el.parentElement.MaterialCheckbox.checkToggleState();
            }, 0);
        },
        'subredditData.flair.championMasteryEnabled': function() {
            setTimeout(function() {
                var el = document.getElementById('checkbox-mastery-emblem');
                if (el) el.parentElement.MaterialCheckbox.checkToggleState();
            }, 0);
        },
        'subredditData.flair.championMasteryTextEnabled': function() {
            setTimeout(function() {
                var el = document.getElementById('checkbox-mastery-points');
                if (el) el.parentElement.MaterialCheckbox.checkToggleState();
            }, 0);
        },
        'subredditData.flair.prestigeEnabled': function() {
            setTimeout(function() {
                var el = document.getElementById('checkbox-prestige');
                if (el) el.parentElement.MaterialCheckbox.checkToggleState();
            }, 0);
        },
        'subredditData.flair.flairText': function() {
            setTimeout(function() {
                var el = document.getElementById('flair-text');
                if (el) el.parentElement.MaterialTextfield.checkDirty();
            }, 0);
        }
    },
    mounted: function() {
        modalService.registerDialog(this.$el);
    }
});

var backgroundService = {
    state: {
        championId: 99,
        skinId: 8,
        allSkins: {"1000":"Annie","1001":"Goth Annie","1002":"Red Riding Annie","1003":"Annie in Wonderland","1004":"Prom Queen Annie","1005":"Frostfire Annie","1006":"Reverse Annie","1007":"FrankenTibbers Annie","1008":"Panda Annie","1009":"Sweetheart Annie","1010":"Hextech Annie","1011":"Super Galaxy Annie","2000":"Olaf","2001":"Forsaken Olaf","2002":"Glacial Olaf","2003":"Brolaf","2004":"Pentakill Olaf","2005":"Marauder Olaf","2006":"Butcher Olaf","2015":"SKT T1 Olaf","3000":"Galio","3001":"Enchanted Galio","3002":"Hextech Galio","3003":"Commando Galio","3004":"Gatekeeper Galio","3005":"Debonair Galio","3006":"Birdio","4000":"Twisted Fate","4001":"PAX Twisted Fate","4002":"Jack of Hearts Twisted Fate","4003":"The Magnificent Twisted Fate","4004":"Tango Twisted Fate","4005":"High Noon Twisted Fate","4006":"Musketeer Twisted Fate","4007":"Underworld Twisted Fate","4008":"Red Card Twisted Fate","4009":"Cutpurse Twisted Fate","4010":"Blood Moon Twisted Fate","4011":"Pulsefire Twisted Fate","5000":"Xin Zhao","5001":"Commando Xin Zhao","5002":"Imperial Xin Zhao","5003":"Viscero Xin Zhao","5004":"Winged Hussar Xin Zhao","5005":"Warring Kingdoms Xin Zhao","5006":"Secret Agent Xin Zhao","5013":"Dragonslayer Xin Zhao","5020":"Cosmic Defender Xin Zhao","6000":"Urgot","6001":"Giant Enemy Crabgot","6002":"Butcher Urgot","6003":"Battlecast Urgot","6009":"High Noon Urgot","7000":"LeBlanc","7001":"Wicked LeBlanc","7002":"Prestigious LeBlanc","7003":"Mistletoe LeBlanc","7004":"Ravenborn LeBlanc","7005":"Elderwood LeBlanc","7012":"Program LeBlanc","8000":"Vladimir","8001":"Count Vladimir","8002":"Marquis Vladimir","8003":"Nosferatu Vladimir","8004":"Vandal Vladimir","8005":"Blood Lord Vladimir","8006":"Soulstealer Vladimir","8007":"Academy Vladimir","8008":"Dark Waters Vladimir","9000":"Fiddlesticks","9001":"Spectral Fiddlesticks","9002":"Union Jack Fiddlesticks","9003":"Bandito Fiddlesticks","9004":"Pumpkinhead Fiddlesticks","9005":"Fiddle Me Timbers","9006":"Surprise Party Fiddlesticks","9007":"Dark Candy Fiddlesticks","9008":"Risen Fiddlesticks","9009":"Praetorian Fiddlesticks","10000":"Kayle","10001":"Silver Kayle","10002":"Viridian Kayle","10003":"Unmasked Kayle","10004":"Battleborn Kayle","10005":"Judgment Kayle","10006":"Aether Wing Kayle","10007":"Riot Kayle","10008":"Iron Inquisitor Kayle","10009":"Pentakill Kayle","11000":"Master Yi","11001":"Assassin Master Yi","11002":"Chosen Master Yi","11003":"Ionia Master Yi","11004":"Samurai Yi","11005":"Headhunter Master Yi","11009":"PROJECT: Yi","11010":"Cosmic Blade Master Yi","11011":"Eternal Sword Yi","11017":"Snow Man Yi","12000":"Alistar","12001":"Black Alistar","12002":"Golden Alistar","12003":"Matador Alistar","12004":"Longhorn Alistar","12005":"Unchained Alistar","12006":"Infernal Alistar","12007":"Sweeper Alistar","12008":"Marauder Alistar","12009":"SKT T1 Alistar","12010":"Moo Cow Alistar","12019":"Hextech Alistar","13000":"Ryze","13001":"Young Ryze","13002":"Tribal Ryze","13003":"Uncle Ryze","13004":"Triumphant Ryze","13005":"Professor Ryze","13006":"Zombie Ryze","13007":"Dark Crystal Ryze","13008":"Pirate Ryze","13009":"Ryze Whitebeard","13010":"SKT T1 Ryze","14000":"Sion","14001":"Hextech Sion","14002":"Barbarian Sion","14003":"Lumberjack Sion","14004":"Warmonger Sion","14005":"Mecha Zero Sion","15000":"Sivir","15001":"Warrior Princess Sivir","15002":"Spectacular Sivir","15003":"Huntress Sivir","15004":"Bandit Sivir","15005":"PAX Sivir","15006":"Snowstorm Sivir","15007":"Warden Sivir","15008":"Victorious Sivir","15009":"Neo PAX Sivir","15010":"Pizza Delivery Sivir","15016":"Blood Moon Sivir","16000":"Soraka","16001":"Dryad Soraka","16002":"Divine Soraka","16003":"Celestine Soraka","16004":"Reaper Soraka","16005":"Order of the Banana Soraka","16006":"Program Soraka","16007":"Star Guardian Soraka","16008":"Pajama Guardian Soraka","16009":"Winter Wonder Soraka","17000":"Teemo","17001":"Happy Elf Teemo","17002":"Recon Teemo","17003":"Badger Teemo","17004":"Astronaut Teemo","17005":"Cottontail Teemo","17006":"Super Teemo","17007":"Panda Teemo","17008":"Omega Squad Teemo","17014":"Little Devil Teemo","17018":"Beemo","18000":"Tristana","18001":"Riot Girl Tristana","18002":"Earnest Elf Tristana","18003":"Firefighter Tristana","18004":"Guerilla Tristana","18005":"Buccaneer Tristana","18006":"Rocket Girl Tristana","18010":"Dragon Trainer Tristana","18011":"Bewitching Tristana","18012":"Omega Squad Tristana","19000":"Warwick","19001":"Grey Warwick","19002":"Urf the Manatee","19003":"Big Bad Warwick","19004":"Tundra Hunter Warwick","19005":"Feral Warwick","19006":"Firefang Warwick","19007":"Hyena Warwick","19008":"Marauder Warwick","19009":"Urfwick","19010":"Lunar Guardian Warwick","20000":"Nunu & Willump","20001":"Sasquatch Nunu & Willump","20002":"Workshop Nunu & Willump","20003":"Grungy Nunu & Willump","20004":"Nunu & Willump Bot","20005":"Demolisher Nunu & Willump","20006":"TPA Nunu & Willump","20007":"Zombie Nunu & Willump","21000":"Miss Fortune","21001":"Cowgirl Miss Fortune","21002":"Waterloo Miss Fortune","21003":"Secret Agent Miss Fortune","21004":"Candy Cane Miss Fortune","21005":"Road Warrior Miss Fortune","21006":"Mafia Miss Fortune","21007":"Arcade Miss Fortune","21008":"Captain Fortune","21009":"Pool Party Miss Fortune","21015":"Star Guardian Miss Fortune","21016":"Gun Goddess Miss Fortune","21017":"Pajama Guardian Miss Fortune","22000":"Ashe","22001":"Freljord Ashe","22002":"Sherwood Forest Ashe","22003":"Woad Ashe","22004":"Queen Ashe","22005":"Amethyst Ashe","22006":"Heartseeker Ashe","22007":"Marauder Ashe","22008":"PROJECT: Ashe","22009":"Championship Ashe","22011":"Cosmic Queen Ashe","23000":"Tryndamere","23001":"Highland Tryndamere","23002":"King Tryndamere","23003":"Viking Tryndamere","23004":"Demonblade Tryndamere","23005":"Sultan Tryndamere","23006":"Warring Kingdoms Tryndamere","23007":"Nightmare Tryndamere","23008":"Beast Hunter Tryndamere","23009":"Chemtech Tryndamere","24000":"Jax","24001":"The Mighty Jax","24002":"Vandal Jax","24003":"Angler Jax","24004":"PAX Jax","24005":"Jaximus","24006":"Temple Jax","24007":"Nemesis Jax","24008":"SKT T1 Jax","24012":"Warden Jax","24013":"God Staff Jax","25000":"Morgana","25001":"Exiled Morgana","25002":"Sinful Succulence Morgana","25003":"Blade Mistress Morgana","25004":"Blackthorn Morgana","25005":"Ghost Bride Morgana","25006":"Victorious Morgana","25010":"Lunar Wraith Morgana","25011":"Bewitching Morgana","26000":"Zilean","26001":"Old Saint Zilean","26002":"Groovy Zilean","26003":"Shurima Desert Zilean","26004":"Time Machine Zilean","26005":"Blood Moon Zilean","27000":"Singed","27001":"Riot Squad Singed","27002":"Hextech Singed","27003":"Surfer Singed","27004":"Mad Scientist Singed","27005":"Augmented Singed","27006":"Snow Day Singed","27007":"SSW Singed","27008":"Black Scourge Singed","27009":"Beekeeper Singed","28000":"Evelynn","28001":"Shadow Evelynn","28002":"Masquerade Evelynn","28003":"Tango Evelynn","28004":"Safecracker Evelynn","28005":"Blood Moon Evelynn","28006":"K/DA Evelynn","29000":"Twitch","29001":"Kingpin Twitch","29002":"Whistler Village Twitch","29003":"Medieval Twitch","29004":"Gangster Twitch","29005":"Vandal Twitch","29006":"Pickpocket Twitch","29007":"SSW Twitch","29008":"Omega Squad Twitch","29012":"Ice King Twitch","30000":"Karthus","30001":"Phantom Karthus","30002":"Statue of Karthus","30003":"Grim Reaper Karthus","30004":"Pentakill Karthus","30005":"Fnatic Karthus","30009":"Karthus Lightsbane","31000":"Cho'Gath","31001":"Nightmare Cho'Gath","31002":"Gentleman Cho'Gath","31003":"Loch Ness Cho'Gath","31004":"Jurassic Cho'Gath","31005":"Battlecast Prime Cho'Gath","31006":"Prehistoric Cho'Gath","31007":"Dark Star Cho'Gath","32000":"Amumu","32001":"Pharaoh Amumu","32002":"Vancouver Amumu","32003":"Emumu","32004":"Re-Gifted Amumu","32005":"Almost-Prom King Amumu","32006":"Little Knight Amumu","32007":"Sad Robot Amumu","32008":"Surprise Party Amumu","32017":"Infernal Amumu","33000":"Rammus","33001":"King Rammus","33002":"Chrome Rammus","33003":"Molten Rammus","33004":"Freljord Rammus","33005":"Ninja Rammus","33006":"Full Metal Rammus","33007":"Guardian of the Sands Rammus","33008":"Sweeper Rammus","34000":"Anivia","34001":"Team Spirit Anivia","34002":"Bird of Prey Anivia","34003":"Noxus Hunter Anivia","34004":"Hextech Anivia","34005":"Blackfrost Anivia","34006":"Prehistoric Anivia","34007":"Festival Queen Anivia","35000":"Shaco","35001":"Mad Hatter Shaco","35002":"Royal Shaco","35003":"Nutcracko","35004":"Workshop Shaco","35005":"Asylum Shaco","35006":"Masked Shaco","35007":"Wild Card Shaco","36000":"Dr. Mundo","36001":"Toxic Dr. Mundo","36002":"Mr. Mundoverse","36003":"Corporate Mundo","36004":"Mundo Mundo","36005":"Executioner Mundo","36006":"Rageborn Mundo","36007":"TPA Mundo","36008":"Pool Party Mundo","36009":"El Macho Mundo","36010":"Frozen Prince Mundo","37000":"Sona","37001":"Muse Sona","37002":"Pentakill Sona","37003":"Silent Night Sona","37004":"Guqin Sona","37005":"Arcade Sona","37006":"DJ Sona","37007":"Sweetheart Sona","37009":"Odyssey Sona","38000":"Kassadin","38001":"Festival Kassadin","38002":"Deep One Kassadin","38003":"Pre-Void Kassadin","38004":"Harbinger Kassadin","38005":"Cosmic Reaver Kassadin","39000":"Irelia","39001":"Nightblade Irelia","39002":"Aviator Irelia","39003":"Infiltrator Irelia","39004":"Frostblade Irelia","39005":"Order of the Lotus Irelia","39006":"Divine Sword Irelia","40000":"Janna","40001":"Tempest Janna","40002":"Hextech Janna","40003":"Frost Queen Janna","40004":"Victorious Janna","40005":"Forecast Janna","40006":"Fnatic Janna","40007":"Star Guardian Janna","40008":"Sacred Sword Janna","40013":"Bewitching Janna","41000":"Gangplank","41001":"Spooky Gangplank","41002":"Minuteman Gangplank","41003":"Sailor Gangplank","41004":"Toy Soldier Gangplank","41005":"Special Forces Gangplank","41006":"Sultan Gangplank","41007":"Captain Gangplank","41008":"Dreadnova Gangplank","41014":"Pool Party Gangplank","42000":"Corki","42001":"UFO Corki","42002":"Ice Toboggan Corki","42003":"Red Baron Corki","42004":"Hot Rod Corki","42005":"Urfrider Corki","42006":"Dragonwing Corki","42007":"Fnatic Corki","42008":"Arcade Corki","43000":"Karma","43001":"Sun Goddess Karma","43002":"Sakura Karma","43003":"Traditional Karma","43004":"Order of the Lotus Karma","43005":"Warden Karma","43006":"Winter Wonder Karma","43007":"Conqueror Karma","44000":"Taric","44001":"Emerald Taric","44002":"Armor of the Fifth Age Taric","44003":"Bloodstone Taric","44004":"Pool Party Taric","45000":"Veigar","45001":"White Mage Veigar","45002":"Curling Veigar","45003":"Veigar Greybeard","45004":"Leprechaun Veigar","45005":"Baron Von Veigar","45006":"Superb Villain Veigar","45007":"Bad Santa Veigar","45008":"Final Boss Veigar","45009":"Omega Squad Veigar","48000":"Trundle","48001":"Lil' Slugger Trundle","48002":"Junkyard Trundle","48003":"Traditional Trundle","48004":"Constable Trundle","48005":"Worldbreaker Trundle","50000":"Swain","50001":"Northern Front Swain","50002":"Bilgewater Swain","50003":"Tyrant Swain","50004":"Dragon Master Swain","51000":"Caitlyn","51001":"Resistance Caitlyn","51002":"Sheriff Caitlyn","51003":"Safari Caitlyn","51004":"Arctic Warfare Caitlyn","51005":"Officer Caitlyn","51006":"Headhunter Caitlyn","51010":"Lunar Wraith Caitlyn","51011":"Pulsefire Caitlyn","51013":"Pool Party Caitlyn","53000":"Blitzcrank","53001":"Rusty Blitzcrank","53002":"Goalkeeper Blitzcrank","53003":"Boom Boom Blitzcrank","53004":"Piltover Customs Blitzcrank","53005":"Definitely Not Blitzcrank","53006":"iBlitzcrank","53007":"Riot Blitzcrank","53011":"Battle Boss Blitzcrank","53020":"Lancer Rogue Blitzcrank","53021":"Lancer Paragon Blitzcrank","54000":"Malphite","54001":"Shamrock Malphite","54002":"Coral Reef Malphite","54003":"Marble Malphite","54004":"Obsidian Malphite","54005":"Glacial Malphite","54006":"Mecha Malphite","54007":"Ironside Malphite","54016":"Odyssey Malphite","55000":"Katarina","55001":"Mercenary Katarina","55002":"Red Card Katarina","55003":"Bilgewater Katarina","55004":"Kitty Cat Katarina","55005":"High Command Katarina","55006":"Sandstorm Katarina","55007":"Slay Belle Katarina","55008":"Warring Kingdoms Katarina","55009":"PROJECT: Katarina","55010":"Death Sworn Katarina","56000":"Nocturne","56001":"Frozen Terror Nocturne","56002":"Void Nocturne","56003":"Ravager Nocturne","56004":"Haunting Nocturne","56005":"Eternum Nocturne","56006":"Cursed Revenant Nocturne","57000":"Maokai","57001":"Charred Maokai","57002":"Totemic Maokai","57003":"Festive Maokai","57004":"Haunted Maokai","57005":"Goalkeeper Maokai","57006":"Meowkai","57007":"Victorious Maokai","58000":"Renekton","58001":"Galactic Renekton","58002":"Outback Renekton","58003":"Bloodfury Renekton","58004":"Rune Wars Renekton","58005":"Scorched Earth Renekton","58006":"Pool Party Renekton","58007":"Prehistoric Renekton","58008":"SKT T1 Renekton","58009":"Renektoy","58017":"Hextech Renekton","59000":"Jarvan IV","59001":"Commando Jarvan IV","59002":"Dragonslayer Jarvan IV","59003":"Darkforge Jarvan IV","59004":"Victorious Jarvan IV","59005":"Warring Kingdoms Jarvan IV","59006":"Fnatic Jarvan IV","59007":"Dark Star Jarvan IV","59008":"SSG Jarvan IV","60000":"Elise","60001":"Death Blossom Elise","60002":"Victorious Elise","60003":"Blood Moon Elise","60004":"SKT T1 Elise","60005":"Super Galaxy Elise","61000":"Orianna","61001":"Gothic Orianna","61002":"Sewn Chaos Orianna","61003":"Bladecraft Orianna","61004":"TPA Orianna","61005":"Winter Wonder Orianna","61006":"Heartseeker Orianna","61007":"Dark Star Orianna","61008":"Victorious Orianna","62000":"Wukong","62001":"Volcanic Wukong","62002":"General Wukong","62003":"Jade Dragon Wukong","62004":"Underworld Wukong","62005":"Radiant Wukong","62006":"Lancer Stratus Wukong","63000":"Brand","63001":"Apocalyptic Brand","63002":"Vandal Brand","63003":"Cryocore Brand","63004":"Zombie Brand","63005":"Spirit Fire Brand","63006":"Battle Boss Brand","64000":"Lee Sin","64001":"Traditional Lee Sin","64002":"Acolyte Lee Sin","64003":"Dragon Fist Lee Sin","64004":"Muay Thai Lee Sin","64005":"Pool Party Lee Sin","64006":"SKT T1 Lee Sin","64010":"Knockout Lee Sin","64011":"God Fist Lee Sin","64012":"Playmaker Lee Sin","67000":"Vayne","67001":"Vindicator Vayne","67002":"Aristocrat Vayne","67003":"Dragonslayer Vayne","67004":"Heartseeker Vayne","67005":"SKT T1 Vayne","67006":"Arclight Vayne","67010":"Soulstealer Vayne","67011":"PROJECT: Vayne","67012":"Firecracker Vayne","67013":"Firecracker Vayne Prestige Edition","68000":"Rumble","68001":"Rumble in the Jungle","68002":"Bilgerat Rumble","68003":"Super Galaxy Rumble","68004":"Badlands Baron Rumble","69000":"Cassiopeia","69001":"Desperada Cassiopeia","69002":"Siren Cassiopeia","69003":"Mythic Cassiopeia","69004":"Jade Fang Cassiopeia","69008":"Eternum Cassiopeia","72000":"Skarner","72001":"Sandscourge Skarner","72002":"Earthrune Skarner","72003":"Battlecast Alpha Skarner","72004":"Guardian of the Sands Skarner","74000":"Heimerdinger","74001":"Alien Invader Heimerdinger","74002":"Blast Zone Heimerdinger","74003":"Piltover Customs Heimerdinger","74004":"Snowmerdinger","74005":"Hazmat Heimerdinger","74006":"Dragon Trainer Heimerdinger","75000":"Nasus","75001":"Galactic Nasus","75002":"Pharaoh Nasus","75003":"Dreadknight Nasus","75004":"Riot K-9 Nasus","75005":"Infernal Nasus","75006":"Archduke Nasus","75010":"Worldbreaker Nasus","75011":"Lunar Guardian Nasus","76000":"Nidalee","76001":"Snow Bunny Nidalee","76002":"Leopard Nidalee","76003":"French Maid Nidalee","76004":"Pharaoh Nidalee","76005":"Bewitching Nidalee","76006":"Headhunter Nidalee","76007":"Warring Kingdoms Nidalee","76008":"Challenger Nidalee","76009":"Super Galaxy Nidalee","77000":"Udyr","77001":"Black Belt Udyr","77002":"Primal Udyr","77003":"Spirit Guard Udyr","77004":"Definitely Not Udyr","78000":"Poppy","78001":"Noxus Poppy","78002":"Lollipoppy","78003":"Blacksmith Poppy","78004":"Ragdoll Poppy","78005":"Battle Regalia Poppy","78006":"Scarlet Hammer Poppy","78007":"Star Guardian Poppy","78014":"Snow Fawn Poppy","78015":"Hextech Poppy","79000":"Gragas","79001":"Scuba Gragas","79002":"Hillbilly Gragas","79003":"Santa Gragas","79004":"Gragas, Esq.","79005":"Vandal Gragas","79006":"Oktoberfest Gragas","79007":"Superfan Gragas","79008":"Fnatic Gragas","79009":"Gragas Caskbreaker","79010":"Arctic Ops Gragas","80000":"Pantheon","80001":"Myrmidon Pantheon","80002":"Ruthless Pantheon","80003":"Perseus Pantheon","80004":"Full Metal Pantheon","80005":"Glaive Warrior Pantheon","80006":"Dragonslayer Pantheon","80007":"Slayer Pantheon","80008":"Baker Pantheon","81000":"Ezreal","81001":"Nottingham Ezreal","81002":"Striker Ezreal","81003":"Frosted Ezreal","81004":"Explorer Ezreal","81005":"Pulsefire Ezreal","81006":"TPA Ezreal","81007":"Debonair Ezreal","81008":"Ace of Spades Ezreal","81009":"Arcade Ezreal","81018":"Star Guardian Ezreal","81019":"SSG Ezreal","81020":"Pajama Guardian Ezreal","82000":"Mordekaiser","82001":"Dragon Knight Mordekaiser","82002":"Infernal Mordekaiser","82003":"Pentakill Mordekaiser","82004":"Lord Mordekaiser","82005":"King of Clubs Mordekaiser","83000":"Yorick","83001":"Undertaker Yorick","83002":"Pentakill Yorick","83003":"Arclight Yorick","84000":"Akali","84001":"Stinger Akali","84002":"Infernal Akali","84003":"All-star Akali","84004":"Nurse Akali","84005":"Blood Moon Akali","84006":"Silverfang Akali","84007":"Headhunter Akali","84008":"Sashimi Akali","84009":"K/DA Akali","84013":"K/DA Akali Prestige Edition","85000":"Kennen","85001":"Deadly Kennen","85002":"Swamp Master Kennen","85003":"Karate Kennen","85004":"Kennen M.D.","85005":"Arctic Ops Kennen","85006":"Blood Moon Kennen","85007":"Super Kennen","86000":"Garen","86001":"Sanguine Garen","86002":"Desert Trooper Garen","86003":"Commando Garen","86004":"Dreadknight Garen","86005":"Rugged Garen","86006":"Steel Legion Garen","86010":"Rogue Admiral Garen","86011":"Warring Kingdoms Garen","86013":"God-King Garen","89000":"Leona","89001":"Valkyrie Leona","89002":"Defender Leona","89003":"Iron Solari Leona","89004":"Pool Party Leona","89008":"PROJECT: Leona","89009":"Barbecue Leona","89010":"Solar Eclipse Leona","89011":"Lunar Eclipse Leona","90000":"Malzahar","90001":"Vizier Malzahar","90002":"Shadow Prince Malzahar","90003":"Djinn Malzahar","90004":"Overlord Malzahar","90005":"Snow Day Malzahar","90006":"Battle Boss Malzahar","90007":"Hextech Malzahar","91000":"Talon","91001":"Renegade Talon","91002":"Crimson Elite Talon","91003":"Dragonblade Talon","91004":"SSW Talon","91005":"Blood Moon Talon","91012":"Enduring Sword Talon","92000":"Riven","92001":"Redeemed Riven","92002":"Crimson Elite Riven","92003":"Battle Bunny Riven","92004":"Championship Riven","92005":"Dragonblade Riven","92006":"Arcade Riven","92007":"Championship Riven 2016","92016":"Dawnbringer Riven","92018":"Pulsefire Riven","96000":"Kog'Maw","96001":"Caterpillar Kog'Maw","96002":"Sonoran Kog'Maw","96003":"Monarch Kog'Maw","96004":"Reindeer Kog'Maw","96005":"Lion Dance Kog'Maw","96006":"Deep Sea Kog'Maw","96007":"Jurassic Kog'Maw","96008":"Battlecast Kog'Maw","96009":"Pug'Maw","96010":"Hextech Kog'Maw","98000":"Shen","98001":"Frozen Shen","98002":"Yellow Jacket Shen","98003":"Surgeon Shen","98004":"Blood Moon Shen","98005":"Warlord Shen","98006":"TPA Shen","98015":"Pulsefire Shen","99000":"Lux","99001":"Sorceress Lux","99002":"Spellthief Lux","99003":"Commando Lux","99004":"Imperial Lux","99005":"Steel Legion Lux","99006":"Star Guardian Lux","99007":"Elementalist Lux","99008":"Lunar Empress Lux","99014":"Pajama Guardian Lux","101000":"Xerath","101001":"Runeborn Xerath","101002":"Battlecast Xerath","101003":"Scorched Earth Xerath","101004":"Guardian of the Sands Xerath","102000":"Shyvana","102001":"Ironscale Shyvana","102002":"Boneclaw Shyvana","102003":"Darkflame Shyvana","102004":"Ice Drake Shyvana","102005":"Championship Shyvana","102006":"Super Galaxy Shyvana","103000":"Ahri","103001":"Dynasty Ahri","103002":"Midnight Ahri","103003":"Foxfire Ahri","103004":"Popstar Ahri","103005":"Challenger Ahri","103006":"Academy Ahri","103007":"Arcade Ahri","103014":"Star Guardian Ahri","103015":"K/DA Ahri","104000":"Graves","104001":"Hired Gun Graves","104002":"Jailbreak Graves","104003":"Mafia Graves","104004":"Riot Graves","104005":"Pool Party Graves","104006":"Cutthroat Graves","104007":"Snow Day Graves","104014":"Victorious Graves","104018":"Praetorian Graves","105000":"Fizz","105001":"Atlantean Fizz","105002":"Tundra Fizz","105003":"Fisherman Fizz","105004":"Void Fizz","105008":"Cottontail Fizz","105009":"Super Galaxy Fizz","105010":"Omega Squad Fizz","106000":"Volibear","106001":"Thunder Lord Volibear","106002":"Northern Storm Volibear","106003":"Runeguard Volibear","106004":"Captain Volibear","106005":"El Rayo Volibear","107000":"Rengar","107001":"Headhunter Rengar","107002":"Night Hunter Rengar","107003":"SSW Rengar","107008":"Mecha Rengar","110000":"Varus","110001":"Blight Crystal Varus","110002":"Arclight Varus","110003":"Arctic Ops Varus","110004":"Heartseeker Varus","110005":"Varus Swiftbolt","110006":"Dark Star Varus","110007":"Conqueror Varus","111000":"Nautilus","111001":"Abyssal Nautilus","111002":"Subterranean Nautilus","111003":"AstroNautilus","111004":"Warden Nautilus","111005":"Worldbreaker Nautilus","112000":"Viktor","112001":"Full Machine Viktor","112002":"Prototype Viktor","112003":"Creator Viktor","112004":"Death Sworn Viktor","113000":"Sejuani","113001":"Sabretusk Sejuani","113002":"Darkrider Sejuani","113003":"Traditional Sejuani","113004":"Bear Cavalry Sejuani","113005":"Poro Rider Sejuani","113006":"Beast Hunter Sejuani","113007":"Sejuani Dawnchaser","113008":"Firecracker Sejuani","114000":"Fiora","114001":"Royal Guard Fiora","114002":"Nightraven Fiora","114003":"Headmistress Fiora","114004":"PROJECT: Fiora","114005":"Pool Party Fiora","114022":"Soaring Sword Fiora","114023":"Heartpiercer Fiora","115000":"Ziggs","115001":"Mad Scientist Ziggs","115002":"Major Ziggs","115003":"Pool Party Ziggs","115004":"Snow Day Ziggs","115005":"Master Arcanist Ziggs","115006":"Battle Boss Ziggs","115007":"Odyssey Ziggs","117000":"Lulu","117001":"Bittersweet Lulu","117002":"Wicked Lulu","117003":"Dragon Trainer Lulu","117004":"Winter Wonder Lulu","117005":"Pool Party Lulu","117006":"Star Guardian Lulu","117014":"Cosmic Enchantress Lulu","117015":"Pajama Guardian Lulu","119000":"Draven","119001":"Soul Reaver Draven","119002":"Gladiator Draven","119003":"Primetime Draven","119004":"Pool Party Draven","119005":"Beast Hunter Draven","119006":"Draven Draven","119012":"Santa Draven","120000":"Hecarim","120001":"Blood Knight Hecarim","120002":"Reaper Hecarim","120003":"Headless Hecarim","120004":"Arcade Hecarim","120005":"Elderwood Hecarim","120006":"Worldbreaker Hecarim","120007":"Lancer Zero Hecarim","121000":"Kha'Zix","121001":"Mecha Kha'Zix","121002":"Guardian of the Sands Kha'Zix","121003":"Death Blossom Kha'Zix","121004":"Dark Star Kha'Zix","121011":"Championship Kha'Zix","122000":"Darius","122001":"Lord Darius","122002":"Bioforge Darius","122003":"Woad King Darius","122004":"Dunkmaster Darius","122008":"Academy Darius","122014":"Dreadnova Darius","122015":"God-King Darius","126000":"Jayce","126001":"Full Metal Jayce","126002":"Debonair Jayce","126003":"Forsaken Jayce","126004":"Jayce Brighthammer","127000":"Lissandra","127001":"Bloodstone Lissandra","127002":"Blade Queen Lissandra","127003":"Program Lissandra","127004":"Coven Lissandra","131000":"Diana","131001":"Dark Valkyrie Diana","131002":"Lunar Goddess Diana","131003":"Infernal Diana","131011":"Blood Moon Diana","131012":"Dark Waters Diana","133000":"Quinn","133001":"Phoenix Quinn","133002":"Woad Scout Quinn","133003":"Corsair Quinn","133004":"Heartseeker Quinn","134000":"Syndra","134001":"Justicar Syndra","134002":"Atlantean Syndra","134003":"Queen of Diamonds Syndra","134004":"Snow Day Syndra","134005":"SKT T1 Syndra","134006":"Star Guardian Syndra","136000":"Aurelion Sol","136001":"Ashen Lord Aurelion Sol","136002":"Mecha Aurelion Sol","141000":"Kayn","141001":"Soulhunter Kayn","141002":"Odyssey Kayn","142000":"Zoe","142001":"Cyber Pop Zoe","142002":"Pool Party Zoe","143000":"Zyra","143001":"Wildfire Zyra","143002":"Haunted Zyra","143003":"SKT T1 Zyra","143004":"Dragon Sorceress Zyra","145000":"Kai'Sa","145001":"Bullet Angel Kai'Sa","145014":"K/DA Kai'Sa","145015":"K/DA Kai'Sa Prestige Edition","150000":"Gnar","150001":"Dino Gnar","150002":"Gentleman Gnar","150003":"Snow Day Gnar","150004":"El León Gnar","150013":"Super Galaxy Gnar","150014":"SSG Gnar","154000":"Zac","154001":"Special Weapon Zac","154002":"Pool Party Zac","154006":"SKT T1 Zac","157000":"Yasuo","157001":"High Noon Yasuo","157002":"PROJECT: Yasuo","157003":"Blood Moon Yasuo","157009":"Nightbringer Yasuo","157010":"Odyssey Yasuo","161000":"Vel'Koz","161001":"Battlecast Vel'Koz","161002":"Arclight Vel'Koz","161003":"Definitely Not Vel'Koz","163000":"Taliyah","163001":"Freljord Taliyah","163002":"SSG Taliyah","164000":"Camille","164001":"Program Camille","164002":"Coven Camille","201000":"Braum","201001":"Dragonslayer Braum","201002":"El Tigre Braum","201003":"Braum Lionheart","201010":"Santa Braum","201011":"Mafia Braum","202000":"Jhin","202001":"High Noon Jhin","202002":"Blood Moon Jhin","202003":"SKT T1 Jhin","202004":"PROJECT: Jhin","203000":"Kindred","203001":"Shadowfire Kindred","203002":"Super Galaxy Kindred","222000":"Jinx","222001":"Mafia Jinx","222002":"Firecracker Jinx","222003":"Slayer Jinx","222004":"Star Guardian Jinx","222012":"Ambitious Elf Jinx","222013":"Odyssey Jinx","223000":"Tahm Kench","223001":"Master Chef Tahm Kench","223002":"Urf Kench","223003":"Coin Emperor Tahm Kench","236000":"Lucian","236001":"Hired Gun Lucian","236002":"Striker Lucian","236006":"PROJECT: Lucian","236007":"Heartseeker Lucian","236008":"High Noon Lucian","238000":"Zed","238001":"Shockblade Zed","238002":"SKT T1 Zed","238003":"PROJECT: Zed","238010":"Championship Zed","238011":"Death Sworn Zed","240000":"Kled","240001":"Sir Kled","240002":"Count Kledula","245000":"Ekko","245001":"Sandstorm Ekko","245002":"Academy Ekko","245003":"PROJECT: Ekko","245011":"SKT T1 Ekko","245012":"Trick or Treat Ekko","254000":"Vi","254001":"Neon Strike Vi","254002":"Officer Vi","254003":"Debonair Vi","254004":"Demon Vi","254005":"Warring Kingdoms Vi","254011":"PROJECT: Vi","254012":"Heartbreaker Vi","266000":"Aatrox","266001":"Justicar Aatrox","266002":"Mecha Aatrox","266003":"Sea Hunter Aatrox","266007":"Blood Moon Aatrox","266008":"Blood Moon Aatrox Prestige Edition","267000":"Nami","267001":"Koi Nami","267002":"River Spirit Nami","267003":"Urf the Nami-tee","267007":"Deep Sea Nami","267008":"SKT T1 Nami","267009":"Program Nami","268000":"Azir","268001":"Galactic Azir","268002":"Gravelord Azir","268003":"SKT T1 Azir","268004":"Warring Kingdoms Azir","412000":"Thresh","412001":"Deep Terror Thresh","412002":"Championship Thresh","412003":"Blood Moon Thresh","412004":"SSW Thresh","412005":"Dark Star Thresh","412006":"High Noon Thresh","420000":"Illaoi","420001":"Void Bringer Illaoi","420002":"Resistance Illaoi","421000":"Rek'Sai","421001":"Eternum Rek'Sai","421002":"Pool Party Rek'Sai","427000":"Ivern","427001":"Candy King Ivern","429000":"Kalista","429001":"Blood Moon Kalista","429002":"Championship Kalista","429003":"SKT T1 Kalista","432000":"Bard","432001":"Elderwood Bard","432005":"Snow Day Bard","432006":"Bard Bard","497000":"Rakan","497001":"Cosmic Dawn Rakan","497002":"Sweetheart Rakan","497003":"SSG Rakan","498000":"Xayah","498001":"Cosmic Dusk Xayah","498002":"Sweetheart Xayah","498003":"SSG Xayah","516000":"Ornn","516001":"Thunder Lord Ornn","517000":"Sylas","517001":"Lunar Wraith Sylas","518000":"Neeko","518001":"Winter Wonder Neeko","555000":"Pyke","555001":"Sand Wraith Pyke","555009":"Blood Moon Pyke"}
    },
    getSkinName: function(champSkinId) {
        return this.state.allSkins[champSkinId || (this.state.championId * 1000 + this.state.skinId)];
    }
};

var dialogProfileSettingsApp = new Vue({
    el: '#modal-profile-settings',
    data: {
        backgroundState: backgroundService.state,
        selectedId: null,
        search: backgroundService.getSkinName()
    },
    computed: {
        filteredBackgrounds: function() {
            return Object.keys(this.backgroundState.allSkins)
                .filter(function(champSkinId) {
                    var skinName = this.backgroundState.allSkins[champSkinId];
                    return skinName.toUpperCase().indexOf(this.search.toUpperCase()) >= 0
                        && this.search.length >= 3;
                }.bind(this));
        }
    },
    methods: {
        showDialog: function() {
            this.selectedId = this.backgroundState.championId * 1000 + backgroundService.state.skinId;
            this.search = backgroundService.getSkinName();
            modalService.showDialog(this.$el);
        },
        closeDialog: function() {
            modalService.closeDialog(this.$el);
        },

        skinName: function(champSkinId) {
            return backgroundService.getSkinName(champSkinId);
        },
        select: function(champSkinId) {
            this.selectedId = champSkinId;
        },
        saveBackground: function() {
            //TODO: web request.
            this.backgroundState.championId = (this.selectedId / 1000)|0;
            this.backgroundState.skinId = this.selectedId % 1000;
            this.closeDialog();
        }
    },
    mounted: function() {
        modalService.registerDialog(this.$el);
    }
});

var app = new Vue({
    el: '#app',
    data: {
        profileState: profileApiService.state,
        now: moment() // Updating current time.
    },
    computed: {
        sortedChampions: function() {
            return Object.values(this.profileState.champions)
                .sort(function(a, b) { return b.points - a.points; })
                .filter(function(a) { return a.points > 0; }); //desc
        },
        filteredSummoners: function() {
            return Object.values(this.profileState.summoners)
                .filter(function(a) { return !this.profileState.hiddenSummoners[a.id]; }.bind(this));
        },
        topMasterySummoners: function() {
            return this.filteredSummoners.slice()
                .sort(function(a, b) { return b.totalPoints - a.totalPoints; });
        },
        topRankedSummoners: function() {
            return this.filteredSummoners.slice()
                .sort(function(a, b) { return (b.tier - a.tier) || (a.division - b.division); });
        },
        topMasterySubreddits: function() {
            return Object.values(this.profileState.subreddits)
                .sort(function(a, b) {
                    return this.profileState.champions[b.championId].points - this.profileState.champions[a.championId].points;
                }.bind(this));
        }
    },
    methods: {
        updateSummoner: function(summoner, event) {
            event.target.disabled = 'disabled'; // Disable immediately, don't wait for vue.
            profileApiService.updateSummoner(summoner);
        },
        promptAddSummoner: function() {
            dialogAddSummonerApp.promptAddSummoner();
        },
        promptRemoveSummoner: function(summoner) {
            dialogRemoveSummonerApp.promptRemoveSummoner(summoner);
        },
        editSubredditFlair: function(subredditData, rankData, champData) {
            dialogEditSubredditFlairApp.promptEditSubredditFlair(subredditData, rankData, champData);
        },
        editProfileSettings: function() {
            dialogProfileSettingsApp.showDialog(); //TODO
        },
        isSummonerUpdating: function(summoner) {
            return profileApiService.isSummonerUpdating(summoner);
        }
    },
    filters: {
        prestige: function(num) {
            if (typeof num !== 'number')
                return num;
            if (num > 1e6)
                return num / 1e6 + 'M';
            return num / 1e3 + 'k';
        }
    },
    created: function() {
        window.addEventListener('load', function(event) { profileApiService.poll(); });
        setInterval(function() {
            this.now = moment();
        }.bind(this), 1000); // Update every second.
    }
});

var backgroundApp = new Vue({
    el: '#background-canvas',
    data: {
        backgroundState: backgroundService.state
    }
});
