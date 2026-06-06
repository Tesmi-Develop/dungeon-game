<?xml version="1.0" encoding="UTF-8"?>
<tileset version="1.10" tiledversion="1.12.1" name="utilities" tilewidth="16" tileheight="16" tilecount="4" columns="2">
 <editorsettings>
  <export target="utilities.tsj" format="json"/>
 </editorsettings>
 <image source="images/utilities.png" width="32" height="32"/>
 <tile id="0">
  <properties>
   <property name="Type" value="Wall"/>
  </properties>
 </tile>
 <tile id="1">
  <properties>
   <property name="Type" value="PlayerSpawner"/>
  </properties>
 </tile>
 <tile id="2">
  <properties>
   <property name="Enemy" value="Melee"/>
   <property name="Type" value="EnemySpawner"/>
  </properties>
 </tile>
 <tile id="3">
  <properties>
   <property name="Enemy" value="Melee1"/>
   <property name="Type" value="EnemySpawner"/>
  </properties>
 </tile>
</tileset>
