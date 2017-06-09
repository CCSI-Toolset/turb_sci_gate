#!/usr/bin/env python

import os, sys
import getopt, traceback, time

import pyodbc
import numpy as np


#	A simple Jobs archiver
#
#   Takes jobs from postgres and moves them to mongo
#
#	Kushner, December 2011

def xstr(s):
	if s is None:
		return ''
	r = str(s)
	r = r.replace("\n", "\\n")
	r = r.replace("\r", "\\r")
	r = r.replace("\t", "\\t")
	r = r.replace('"', '""')
	if isinstance(s, str) or isinstance(s, unicode) or isinstance(s, buffer):
		r = '"' + r + '"'
	return r

 


class archiver(object):
	"""Move new jobs from posgres to mongo"""
	
	def __init__(self, args):
#		self.server = "50.18.172.30"
#		self.server = "ec2-50-18-172-30.us-west-1.compute.amazonaws.com"
		self.server = 'localhost'
		self.PWD    = ''
		
		self.conn   = None
		self.cursor = None

		self.mongo = ''		

	def connectDbs(self):
		"""Connect to the databases"""
		
		dataSource = "DRIVER={SQL SERVER};SERVER=%s;DATABASE=%s;UID=%s;PWD=%s" % \
					 	(self.server, "turbine", "kushner", self.PWD)

		dataSource = "DRIVER={SQL SERVER};SERVER=%s;DATABASE=%s" % \
					 	(self.server, "tempturbine")

#		print dataSource
		
		self.conn   = pyodbc.connect(dataSource)
		self.cursor = self.conn.cursor()
		
	def disconnectDbs(self, commit = False):
		"""Disconnect from the database and do a commit or rollback"""
		
		if commit:
			self.conn.commit()
		else:
			self.conn.rollback()
			
		self.conn.close()

	def archive(self):
		rc = 0

		self.connectDb()
		try:
			self._dump()
		except Exception as E:
			rc = 1
			print "Exception... rolling back database"
			self.disconnectDb(commit = False)
			print " "
			print traceback.print_exc(file=sys.stdout)
			
		else:
			self.disconnectDb(commit = True)
			
#		print "goodbye"

		return rc

		
	def _archive(self):
		execute = self.cursor.execute

		execute("use tempturbine")
		
		execute("""
				    select *
				    from SinterProcesses
	               """)
		
		print "Id,Status,Stdout,Stderr,WorkingDir,Configuration,Backup,Input,Output"
		for row in self.cursor:
				print xstr(row.Id) + "|" + xstr(row.Status) + "|" + xstr(row.Stdout) + "|" +\
				      xstr(row.Stderr) + "|" + xstr(row.WorkingDir) + "|" + xstr(row.Configuration) + "|" +\
				      xstr(row.Backup) + "|" + xstr(row.Input) + "|" + xstr(row.Output)
		
####
####


	
####
####
	
if __name__ == '__main__':
	sys.exit(archiver(sys.argv[1:]).archive())

