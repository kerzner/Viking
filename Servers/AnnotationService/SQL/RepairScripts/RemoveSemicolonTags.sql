/****** Script for SelectTopNRows command from SSMS  ******/
update StructureType Set Tags=NULL where  Tags.value('/','varchar(100)') = ';'
update Structure Set Tags=NULL where  Tags.value('/','varchar(100)') = ';'
update StructureLink Set Tags=NULL where  Tags.value('/','varchar(100)') = ';'
update Location  Set Tags=NULL where  Tags.value('/','varchar(100)') = ';'