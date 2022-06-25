fx_version 'cerulean'
game 'gta5'

author 'Golden Development'
description 'Golden Breathalyzer Script C#'
version '1.0.0'

clr_disable_task_scheduler 'yes'

server_scripts {
	'Golden.Breathalyzer.Server.net.dll'
}

client_scripts {
	'Golden.Breathalyzer.Client.net.dll'
}

ui_page 'html/breathalyze.html'

files {
	'Newtonsoft.Json.dll',
	'html/breathalyze.html',
	'html/assets/breathalyzer_ui.png',
	'html/assets/DS-DIGI.ttf',
	'html/assets/scripts.js',
	'html/assets/styles.css'
}