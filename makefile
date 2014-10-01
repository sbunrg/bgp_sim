all:
	xbuild /p:Configuration=Release TestingSolution.sln /t:Clean
	xbuild /p:Configuration=Release TestingSolution.sln
clean:
	xbuild /p:Configuration=Release TestingSolution.sln /t:Clean

